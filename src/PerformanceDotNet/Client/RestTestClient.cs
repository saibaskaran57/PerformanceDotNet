namespace PerformanceDotNet.Client
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using PerformanceDotNet.Models;

    internal sealed class RestTestClient : BaseTestClient, ITestClient
    {
        private readonly string endpoint;
        private readonly Version version;
        private readonly string data;
        private readonly TestMode type;

        private readonly HttpClient httpClient;

        public RestTestClient(string endpoint, Version version, string data, TestMode type, RequestConfiguration configuration)
            : base(configuration)
        {
            this.endpoint = endpoint;
            this.version = version;
            this.data = data;
            this.type = type;

            this.httpClient = new HttpClient(new HttpHandler(this.version));
        }

        public async Task ExecuteAsync()
        {
            switch (this.type)
            {
                case TestMode.Single:
                case TestMode.Chunk:
                case TestMode.Burst:
                    await Send(); break;
                case TestMode.Stream:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task Send()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var content = new StringContent(this.data, Encoding.UTF8, "application/json");

            await Execute(async () =>
            {
                await httpClient.PostAsync(this.endpoint, content).ConfigureAwait(false);
            });
        }
    }
}
