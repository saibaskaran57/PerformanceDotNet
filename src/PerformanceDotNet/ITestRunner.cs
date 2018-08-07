namespace PerformanceDotNet
{
    using System.Threading.Tasks;

    internal interface ITestRunner
    {
        Task Execute();
    }
}