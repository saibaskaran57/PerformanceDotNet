namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.DependencyInjection;
    using PerformanceDotNet.Models;

    internal sealed class SignalrTestClient : BaseTestClient, ITestClient
    {
        private readonly string endpoint;
        private readonly int totalRequest;
        private readonly string data;
        private readonly TestMode type;

        public SignalrTestClient(string endpoint, int totalRequest, string data, TestMode type, RequestConfiguration configuration)
            : base(configuration)
        {
            this.endpoint = endpoint;
            this.totalRequest = totalRequest;
            this.data = data;
            this.type = type;
        }

        public async Task<TestResult> ExecuteAsync()
        {
            var connection = new HubConnectionBuilder()
                 .WithUrl(this.endpoint)
                 .AddMessagePackProtocol()
                 .Build();

            switch (type)
            {
                case TestMode.Single:
                case TestMode.Burst:
                    return await Send(connection);
                case TestMode.Stream:
                    return await SendStream(connection);
                case TestMode.Chunk:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task<TestResult> Send(HubConnection connection)
        {
            var testResult = new TestResult();
            var stopWatch = Stopwatch.StartNew();

            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
            });

            await connection.StartAsync();
            testResult.CollectSetupDuration(stopWatch.ElapsedMilliseconds);

            try
            {
                stopWatch.Restart();

                await Execute(async () =>
                {
                    await connection.InvokeAsync<string>("SendMessage", this.data);
                });

                testResult.CollectTestDuration(stopWatch.ElapsedMilliseconds);
            }
            finally
            {
                stopWatch.Restart();
                await connection.StopAsync();
                testResult.CollectTearDownDuration(stopWatch.ElapsedMilliseconds);
            }

            return testResult;
        }

        private async Task<TestResult> SendStream(HubConnection connection)
        {
            var datas = PrepareData(totalRequest, this.data);

            var testResult = new TestResult();
            var stopWatch = Stopwatch.StartNew();

            await connection.StartAsync();
            testResult.CollectSetupDuration(stopWatch.ElapsedMilliseconds);

            // Warm up the client.
            await ReadStream(connection, datas);

            try
            {
                stopWatch.Restart();

                await Execute(async () =>
                {
                    await ReadStream(connection, datas);
                });

                testResult.CollectTestDuration(stopWatch.ElapsedMilliseconds);
            }
            finally
            {
                stopWatch.Restart();
                await connection.StopAsync();
                testResult.CollectTearDownDuration(stopWatch.ElapsedMilliseconds);
            }

            return testResult;
        }

        private static async Task ReadStream(HubConnection connection, IList<string> datas)
        {
            var channel = await connection
            .StreamAsChannelAsync<string>("Stream", datas, 100, CancellationToken.None)
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