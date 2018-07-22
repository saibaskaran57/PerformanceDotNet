namespace PerformanceDotNet.Factory
{
    using System;
    using System.Net;
    using PerformanceDotNet.Client;
    using PerformanceDotNet.Models;

    internal sealed class TestFactory
    {
        private readonly TestSettings settings;

        public TestFactory(TestSettings settings)
        {
            this.settings = settings;
        }

        public ITestClient Build()
        {
            switch (settings.TestType)
            {
                case TestType.Restv1:
                    return new RestTestClient(settings.TestEndpoint, settings.TotalRequest, HttpVersion.Version11, settings.TestData, settings.TestMode);
                case TestType.Restv2:
                    return new RestTestClient(settings.TestEndpoint, settings.TotalRequest, HttpVersion.Version20, settings.TestData, settings.TestMode);
                case TestType.Signalr:
                    return new SignalrTestClient(settings.TestEndpoint, settings.TotalRequest, settings.TestData, settings.TestMode);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
