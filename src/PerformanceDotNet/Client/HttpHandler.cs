namespace PerformanceDotNet.Client
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class HttpHandler : WinHttpHandler
    {
        private readonly Version version;

        public HttpHandler(Version version)
        {
            this.version = version;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Version = this.version;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
