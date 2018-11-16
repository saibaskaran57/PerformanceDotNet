namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using PerformanceDotNet.Models;

    internal sealed class SignalrTestClient : ITestClient
    {
        private readonly string endpoint;
        private readonly string methodName;
        private readonly int totalRequest;
        private readonly List<object> data;
        private readonly Func<Task> testFunction;
        private HubConnection connection;

        public SignalrTestClient(string endpoint,string methodName, int totalRequest, string data, TestMode type, RequestConfiguration configuration)
        {
            this.endpoint = endpoint;
            this.methodName = methodName;
            this.totalRequest = totalRequest;
            //this.data = JsonConvert.DeserializeObject<List<object>>(data);
            this.data = new List<object>();
            Func <Task> testRunAction;

            switch (type)
            {
                case TestMode.Single:
                case TestMode.Burst:
                    testRunAction = async () =>
                    {
                        await connection.InvokeAsync<string>(this.methodName, "Kevin", "Test").ConfigureAwait(false);
                    };
                    break;
                case TestMode.Stream:
                    var datas = this.data.ToArray();
                    testRunAction = async () =>
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
                    var tasks = new List<Task>();
                    for (int i = 1; i <= configuration.Count; i++)
                    {
                        tasks.Add(testRunAction.Invoke());
                    }

                    await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                };
            }
            else
            {
                testFunction = async () =>
                {
                    for (int i = 1; i <= configuration.Count; i++)
                    {
                        await testRunAction.Invoke().ConfigureAwait(false);
                    }
                };
            }
        }

        public async Task ExecuteAsync()
        {
            connection = new HubConnectionBuilder()
                 .WithUrl(this.endpoint)
                 .Build();

            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
            });

            await connection.StartAsync().ConfigureAwait(false);

            await testFunction.Invoke().ConfigureAwait(false);

            await connection.StopAsync().ConfigureAwait(false);
        }

        private async Task ReadStream(HubConnection connection, IList<object> datas)
        {
            var channel = await connection
            .StreamAsChannelAsync<string>(this.methodName, "Kevin", "Test", 100, CancellationToken.None)
            .ConfigureAwait(false);

            while (await channel.WaitToReadAsync().ConfigureAwait(false))
            {
                while (channel.TryRead(out var message))
                {
                    // Console.WriteLine(message);
                }
            }
        }

        private static IList<string> PrepareData(int count, string data)
        {
            var datas = new List<string>();

            for (int i = 1; i <= count; i++)
            {
                datas.Add(data);
            }

            return datas;
        }
    }
}