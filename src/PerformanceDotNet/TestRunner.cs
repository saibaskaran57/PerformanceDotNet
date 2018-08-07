namespace PerformanceDotNet
{
    using PerformanceDotNet.Factory;
    using PerformanceDotNet.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class TestRunner : ITestRunner
    {
        private readonly TestType testType;

        private readonly TestMode testMode;

        private readonly int testRunCount;

        private readonly int virtualUserCount;

        private readonly TestFactory testFactory;

        public TestRunner(TestType testType, TestMode testMode, int testRunCount, int virtualUserCount, TestFactory testFactory)
        {
            this.testType = testType;
            this.testMode = testMode;
            this.testRunCount = testRunCount;
            this.virtualUserCount = virtualUserCount;
            this.testFactory = testFactory;
        }

        public async Task Execute()
        {
            var client = testFactory.Build();

            var responseTimes = new List<double>();

            Console.WriteLine($"Test {testType}-{testMode} started");

            for (int i = 1; i <= testRunCount; i++)
            {
                Console.WriteLine($"Test Run {i} started");

                var startTime = DateTime.UtcNow;
                await client.ExecuteAsync();
                var endTime = DateTime.UtcNow;
                responseTimes.Add((endTime - startTime).TotalMilliseconds);

                Console.WriteLine($"Test Run {i} completed");
                Console.WriteLine($"Time Taken for Test Run {i} - {(endTime - startTime).TotalMilliseconds}ms");

                Console.WriteLine();
            }

            Console.WriteLine("Test completed");
            Console.WriteLine($"Average Time Taken - {responseTimes.Average()}ms");
        }
    }
}