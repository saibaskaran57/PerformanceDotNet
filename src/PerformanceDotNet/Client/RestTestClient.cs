namespace PerformanceDotNet.Client
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using PerformanceDotNet.Models;

    internal sealed class RestTestClient : BaseTestClient, ITestClient
    {
        private readonly string endpoint;
        private readonly Version version;
        private readonly string data;
        private readonly TestMode type;

        public RestTestClient(string endpoint, Version version, string data, TestMode type, RequestConfiguration configuration)
            : base(configuration)
        {
            this.endpoint = endpoint;
            this.version = version;
            this.data = data;
            this.type = type;
        }

        public async Task<TestResult> ExecuteAsync()
        {
            var testResult = new TestResult();

            var stopwatch = Stopwatch.StartNew();
            var httpClient = new HttpClient(new HttpHandler(this.version));
            testResult.CollectSetupDuration(stopwatch.ElapsedMilliseconds);

            switch (this.type)
            {
                case TestMode.Single:
                case TestMode.Chunk:
                case TestMode.Burst:
                    testResult.CollectTestDuration(await Send(httpClient).ConfigureAwait(false)); break;
                case TestMode.Stream:
                    throw new NotImplementedException();
                default:
                    throw new InvalidOperationException();
            }

            return testResult;
        }

        private async Task<double> Send(HttpClient httpClient)
        {
            // Warm up the client.
            var content = new StringContent(this.data, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(this.endpoint, content).ConfigureAwait(false);

            var stopwatch = Stopwatch.StartNew();

            await Execute(async () =>
            {
                var stringContent = new StringContent(this.data, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(this.endpoint, stringContent, CancellationToken.None).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            });

            return stopwatch.ElapsedMilliseconds;
        }
    }
}
