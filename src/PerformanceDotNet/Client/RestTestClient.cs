namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using PerformanceDotNet.Models;

    internal sealed class RestTestClient : ITestClient
    {
        private readonly string endpoint;
        private readonly int totalRequest;
        private readonly Version version;
        private readonly string data;
        private readonly TestMode type;

        public RestTestClient(string endpoint, int totalRequest, Version version, string data, TestMode type)
        {
            this.endpoint = endpoint;
            this.version = version;
            this.totalRequest = totalRequest;
            this.data = data;
            this.type = type;
        }

        public async Task ExecuteAsync()
        {
            switch (this.type)
            {
                case TestMode.Single:
                    await Send(new HttpClient(new HttpHandler(this.version))); break;
                case TestMode.Chunk:
                    await Send(new HttpClient()); break;
                case TestMode.Burst:
                    await SendBurst(new HttpClient(new HttpHandler(this.version))); break;
                case TestMode.Stream:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task Send(HttpClient client)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var content = new StringContent(this.data, Encoding.UTF8, "application/json");

            for (int i = 1; i <= totalRequest; i++)
            {
                var response = await client.PostAsync(this.endpoint, content);
            }
        }

        private async Task SendBurst(HttpClient client)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var tasks = new List<Task>();

            for (int i = 1; i <= totalRequest; i++)
            {
                tasks.Add(await Task.Factory.StartNew(async ()=>{
                    var content = new StringContent(this.data, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(this.endpoint, content);
                }));
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
