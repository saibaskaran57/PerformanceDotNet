namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Newtonsoft.Json;
    using PerformanceDotNet.Models;

    internal sealed class SignalrTestClient : ITestClient
    {
        private readonly string endpoint;
        private readonly string methodName;
        private readonly string responseMethodName;
        private readonly int totalRequest;
        private readonly Dictionary<string, List<Dictionary<string, object>>> data;
        private readonly Func<Task> testFunction;
        private HubConnection connection;
        private int requestIndex = 0;
        private int numOfRequests = 0;
        private List<Dictionary<string, object>> requestPool;

        public SignalrTestClient(string endpoint, string methodName, string responseMethodName, int totalRequest, string data, TestMode type, RequestConfiguration configuration, long testDuration, long testInterval)
        {
            this.endpoint = endpoint;
            this.methodName = methodName;
            this.responseMethodName = responseMethodName;
            this.totalRequest = totalRequest;
            this.data = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string,object>>>>(data);
            Func <object, Task> testRunAction;

            requestPool = PrepareData(this.data["Request"], configuration.Count);

            switch (type)
            {
                case TestMode.Single:
                case TestMode.Burst:
                    testRunAction = async (request) =>
                    {
                        await connection.InvokeAsync<object>(this.methodName, request).ConfigureAwait(false);
                    };
                    break;
                case TestMode.Stream:
                    var datas = this.data;
                    testRunAction = async (request) =>
                    {
                        await ReadStream(connection, datas).ConfigureAwait(false);
                    };
                    break;
                case TestMode.Chunk:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }

            if (configuration.ExecutionType == ExecutionType.Parallel)
            {
                testFunction = async () =>
                {
                    var startTime = Environment.TickCount;
                    var currentInterval = startTime;

                    var mainTasksList = new List<Task>();
                    Task currentTask;
                    while (currentInterval - startTime < testDuration)
                    {
                        var tasks = new List<Task>();
                        for (int i = 0; i < configuration.Count; i++)
                        {
                            currentTask = testRunAction.Invoke(requestPool[i]);
                            tasks.Add(currentTask);
                            mainTasksList.Add(currentTask);
                        }

                        while (Environment.TickCount - currentInterval < testInterval)
                        {
                        }
                        currentInterval = Environment.TickCount;
                    }

                    await Task.WhenAll(mainTasksList.ToArray()).ConfigureAwait(false);
                };
            }
            else
            {
                testFunction = async () =>
                {
                    for (int i = 0; i < configuration.Count; i++)
                    {
                        await testRunAction.Invoke(requestPool[i]).ConfigureAwait(false);
                    }
                };
            }
        }

        public async Task ExecuteAsync()
        {
            connection = new HubConnectionBuilder()
                 .WithUrl(this.endpoint, options =>
                 {
                     options.Headers.Add("Auth-Token", "TEST");
                 })
                 .Build();

            connection.On<object>(this.responseMethodName, (payload) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(payload));
            });

            await connection.StartAsync().ConfigureAwait(false);

            await testFunction.Invoke().ConfigureAwait(false);

            await connection.StopAsync().ConfigureAwait(false);
        }

        private async Task ReadStream(HubConnection connection, Dictionary<string, List<Dictionary<string, object>>> datas)
        {
            var channel = await connection
            .StreamAsChannelAsync<string>(this.methodName, datas, 100, CancellationToken.None)
            .ConfigureAwait(false);

            while (await channel.WaitToReadAsync().ConfigureAwait(false))
            {
                while (channel.TryRead(out var message))
                {
                    // Console.WriteLine(message);
                }
            }
        }

        private List<Dictionary<string, object>> PrepareData(List<Dictionary<string, object>> requests, int numberOfData)
        {
            var datas = new List<Dictionary<string, object>>();
            int index = 0;

            for (int i = 1; i <= numberOfData; i++)
            {
                datas.Add(requests[index]);
                index++;
                if (index == requests.Count)
                {
                    index = 0;
                }
            }

            numOfRequests = datas.Count;
            return datas;
        }
    }
}