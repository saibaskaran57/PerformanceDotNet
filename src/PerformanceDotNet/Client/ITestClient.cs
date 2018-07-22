namespace PerformanceDotNet.Client
{
    using System.Threading.Tasks;

    internal interface ITestClient
    {
        Task ExecuteAsync();
    }
}
