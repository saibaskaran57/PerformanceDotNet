namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
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

        public async Task ExecuteAsync()
        {
            var connection = new HubConnectionBuilder()
                 .WithUrl(this.endpoint)
                 .AddMessagePackProtocol()
                 .Build();

            switch (type)
            {
                case TestMode.Single:
                case TestMode.Burst:
                    await Send(connection); break;
                case TestMode.Stream:
                    await SendStream(connection); break;
                case TestMode.Chunk:
                    throw new NotImplementedException();
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

            await Execute(async () => 
            {
                await connection.InvokeAsync<string>("SendMessage", this.data);
            });

            await connection.StopAsync();
        }

        private async Task SendStream(HubConnection connection)
        {
            await connection.StartAsync();

            var datas = PrepareData(totalRequest, this.data);

            await Execute(async () => 
            {
                await ReadStream(connection, datas);
            });

            await connection.StopAsync();
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
                    Console.WriteLine(message);
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