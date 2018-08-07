namespace PerformanceDotNet.Client
{
    using PerformanceDotNet.Models;
    using System.Threading.Tasks;

    internal interface ITestClient
    {
        Task<TestResult> ExecuteAsync();
    }
}