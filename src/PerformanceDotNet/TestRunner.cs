namespace PerformanceDotNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using PerformanceDotNet.Factory;
    using PerformanceDotNet.Models;

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
            Console.WriteLine($"Test {testType}-{testMode} started with {virtualUserCount} virtual user count");

            var consolidatedResult = new List<TestResult[]>();

            for (int i = 1; i <= testRunCount; i++)
            {
                Console.WriteLine($"Test Run {i} started");
                var result = await RunForUsers().ConfigureAwait(false);
                PrintResult(result);
                consolidatedResult.Add(result);

                Console.WriteLine($"Test Run {i} completed");
                Console.Write(Environment.NewLine);
            }

            Console.WriteLine("Test completed!");
            PrintConsolidatedResult(consolidatedResult);
        }
        
        private async Task<TestResult[]> RunForUsers()
        {
            var tasks = new List<Task<TestResult>>();

            for (int i = 1; i <= virtualUserCount; i++)
            {
                tasks.Add(Run());
            }

            return await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }

        private async Task<TestResult> Run()
        {
            var client = testFactory.Build();
            return await client.ExecuteAsync().ConfigureAwait(false);
        }

        private static void PrintResult(TestResult[] testResult)
        {
            Console.WriteLine($"Overall: {testResult.Average(x => x.OverallDuration)}ms");

            /*for (int i = 0; i < testResult.Length; i++)
            {
                var result = testResult[i];

                Console.Write(Environment.NewLine);
                Console.WriteLine($"User #{i + 1}");
                Console.WriteLine($"Setup: {result.SetupDuration}ms");
                Console.WriteLine($"Test: {result.TestDuration}ms");
                Console.WriteLine($"TearDown: {result.TearDownDuration}ms");
                Console.WriteLine($"Overall: {result.OverallDuration}ms");
                Console.Write(Environment.NewLine);
            }*/
        }

        private static void PrintConsolidatedResult(List<TestResult[]> result)
        {
            // Flatten the result.
            var results = result.SelectMany(x => x);

            Console.Write(Environment.NewLine);
            Console.WriteLine($"Average Time Taken (ms)");
            Console.WriteLine($"Setup: {results.Average(x => x.SetupDuration)}");
            Console.WriteLine($"Test: {results.Average(x => x.TestDuration)}");
            Console.WriteLine($"TearDown: {results.Average(x => x.TearDownDuration)}");
            Console.WriteLine($"Overall: {results.Average(x => x.OverallDuration)}ms");
        }
    }
}