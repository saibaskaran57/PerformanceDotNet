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

            // Set http client initialization duration.
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
            var stopwatch = Stopwatch.StartNew();

            await Execute(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Post, this.endpoint)
                {
                    Content = new StringContent(this.data, Encoding.UTF8, "application/json"),
                    Version = version
                };

                var response = await httpClient.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
            });

            return stopwatch.ElapsedMilliseconds;
        }
    }
}
