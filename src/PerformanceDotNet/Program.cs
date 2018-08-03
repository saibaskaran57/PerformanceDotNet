namespace PerformanceDotNet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using PerformanceDotNet.Factory;
    using PerformanceDotNet.Models;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var settings = GetAppSettings();
            settings.TestData = GetTestData();

            var client = new TestFactory(settings)
                .Build();

            try
            {
                await RunAsync(
                    settings.TestType.ToString(),
                    settings.TestMode.ToString(),
                    settings.TestRuns,
                    client.ExecuteAsync);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"{Environment.NewLine}{ex.Message}");
                Console.WriteLine("Test terminated with error!");
            }

            Console.ReadLine();
        }

        private static async Task RunAsync(string testType, string testMode, int testRuns, Func<Task> func)
        {
            var responseTimes = new List<double>();

            Console.WriteLine($"Test {testType}-{testMode} started");

            for (int i = 1; i <= testRuns; i++)
            {
                Console.WriteLine($"Test Run {i} started");

                var startTime = DateTime.UtcNow;
                await func.Invoke();
                var endTime = DateTime.UtcNow;
                responseTimes.Add((endTime - startTime).TotalMilliseconds);

                Console.WriteLine($"Test Run {i} completed");
                Console.WriteLine($"Time Taken for Test Run {i} - {(endTime - startTime).TotalMilliseconds}ms");

                Console.WriteLine();
            }

            Console.WriteLine("Test completed");
            Console.WriteLine($"Average Time Taken - {responseTimes.Average()}ms");
        }

        private static TestSettings GetAppSettings()
        {
            var appSettingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            return JsonConvert.DeserializeObject<TestSettings>(File.ReadAllText(appSettingPath));
        }

        private static string GetTestData()
        {
            var requestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Request.txt");
            return File.ReadAllText(requestPath);
        }
    }
}