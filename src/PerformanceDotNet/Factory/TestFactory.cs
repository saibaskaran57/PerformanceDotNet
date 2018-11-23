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
                    return new RestTestClient(settings.TestEndpoint, HttpVersion.Version11, settings.TestData, settings.TestMode, settings.RequestConfiguration);
                case TestType.Restv2:
                    return new RestTestClient(settings.TestEndpoint, HttpVersion.Version20, settings.TestData, settings.TestMode, settings.RequestConfiguration);
                case TestType.Signalr:
                    return new SignalrTestClient(settings.TestEndpoint, settings.StreamMethod, settings.StreamResponseMethod, settings.TotalRequest, settings.TestData, settings.TestMode, settings.RequestConfiguration, settings.TestDurationInMs, settings.TestIntervalInMs, settings.AuthToken);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
