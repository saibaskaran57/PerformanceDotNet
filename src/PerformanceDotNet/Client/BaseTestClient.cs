namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using PerformanceDotNet.Models;

    internal abstract class BaseTestClient
    {
        private readonly RequestConfiguration configuration;

        public BaseTestClient(RequestConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected async Task<double> Execute(Func<Task> testRunAction)
        {
            var stopWatch = Stopwatch.StartNew();

            switch (configuration.ExecutionType)
            {
                case ExecutionType.Sequential:
                    await ExecuteSequential(testRunAction); break;
                case ExecutionType.Parallel:
                    await ExecuteParallel(testRunAction); break;
                default:
                    throw new InvalidOperationException();
            }

            return stopWatch.ElapsedMilliseconds;
        }

        private async Task ExecuteSequential(Func<Task> testRunAction)
        {
            for(int i = 1; i <= configuration.Count; i++)
            {
                await testRunAction.Invoke();
            }
        }

        private async Task ExecuteParallel(Func<Task> testRunAction)
        {
            var tasks = new List<Task>();

            for (int i = 1; i <= configuration.Count; i++)
            {
                tasks.Add(testRunAction.Invoke());
            }

            await Task.WhenAll(tasks.ToArray());
        }
    }
}