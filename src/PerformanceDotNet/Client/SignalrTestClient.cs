namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR.Client;
    using Microsoft.Extensions.DependencyInjection;
    using PerformanceDotNet.Models;

    internal sealed class SignalrTestClient : ITestClient
    {
        private readonly string endpoint;
        private readonly int totalRequest;
        private readonly string data;
        private readonly int parallelismCount;
        private readonly TestMode type;

        public SignalrTestClient(string endpoint, int totalRequest, string data, int parallelismCount, TestMode type)
        {
            this.endpoint = endpoint;
            this.totalRequest = totalRequest;
            this.data = data;
            this.parallelismCount = parallelismCount;
            this.type = type;
        }

        public async Task ExecuteAsync()
        {
            var connection = new HubConnectionBuilder()
                 .WithUrl(this.endpoint)
                 .AddMessagePackProtocol()
                 .Build();

            switch (type)
            {
                case TestMode.Single:
                    await Send(connection); break;
                case TestMode.Stream:
                    await SendStream(connection); break;
                case TestMode.Burst:
                    await SendBurst(connection); break;
                case TestMode.Chunk:
                    await SendChunks(connection); break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task Send(HubConnection connection)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
            });

            await connection.StartAsync();

            for (int i = 1; i <= totalRequest; i++)
            {
                await connection.InvokeAsync<string>("SendMessage", this.data);
            }

            await connection.StopAsync();
        }

        private async Task SendStream(HubConnection connection)
        {
            await connection.StartAsync();

            var datas = PrepareData(totalRequest, this.data);

            var channel = await connection.StreamAsChannelAsync<string>("Stream", datas, 100, CancellationToken.None);

            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out var message))
                {
                   Console.WriteLine(message);
                }
            }

            await connection.StopAsync();
        }

        private async Task SendChunks(HubConnection connection)
        {
            await connection.StartAsync();

            var datas = PrepareData(totalRequest, this.data);

            var tasks = new List<Task>();

            for (int i = 1; i <= parallelismCount; i++)
            {
                tasks.Add(await Task.Factory.StartNew(async () => 
                {
                    var channel = await connection
                    .StreamAsChannelAsync<string>("Stream", datas, 100, CancellationToken.None)
                    .ConfigureAwait(false);

                    await channel.WaitToReadAsync().ConfigureAwait(false);
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            await connection.StopAsync();
        }

        private async Task SendBurst(HubConnection connection)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"{user}: {message}");
            });

            await connection.StartAsync();

            var tasks = new List<Task>();

            for (int i = 1; i <= parallelismCount; i++)
            {
                tasks.Add(await Task.Factory.StartNew(async ()=> {
                    await connection.InvokeAsync<string>("SendMessage", this.data);
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            await connection.StopAsync();
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
