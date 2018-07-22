namespace PerformanceDotNet.Client
{
    using System;
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
                case TestMode.Batch:
                    await Send(new HttpClient()); break;
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
    }
}
