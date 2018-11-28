﻿namespace PerformanceDotNet.Models
{
    internal sealed class TestSettings
    {
        public TestType TestType { get; set; }

        public TestMode TestMode { get; set; }

        public string TestData { get; set; }

        public string TestEndpoint { get; set; }

        public int TestRuns { get; set; }

        public int TotalRequest { get; set; }

        public RequestConfiguration RequestConfiguration { get; set; }

        public long TestIntervalInMs { get; set; }

        public long TestDurationInMs { get; set; }

        public string StreamMethod { get; set; }

        public string StreamResponseMethod { get; set; }

        public string AuthToken { get; set; }

        public bool EnableMessagePackStreaming { get; set; }
    }
}