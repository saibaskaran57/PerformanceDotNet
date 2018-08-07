namespace PerformanceDotNet
{
    using System;
    using System.IO;
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

            var testFactory = new TestFactory(settings);

            ITestRunner testRunner = new TestRunner(
                settings.TestType,
                settings.TestMode,
                settings.TestRuns,
                settings.VirtualUsersCount,
                testFactory);

            try
            {
                await testRunner.Execute();
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine($"{Environment.NewLine}{ex.Message}");
                Console.WriteLine("Test terminated with error!");
            }

            Console.ReadLine();
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