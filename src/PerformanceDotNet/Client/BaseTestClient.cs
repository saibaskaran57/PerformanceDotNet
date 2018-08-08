namespace PerformanceDotNet.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PerformanceDotNet.Models;

    internal abstract class BaseTestClient
    {
        private readonly RequestConfiguration configuration;

        public BaseTestClient(RequestConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected async Task Execute(Func<Task> testRunAction)
        {
            switch (configuration.ExecutionType)
            {
                case ExecutionType.Sequential:
                    await ExecuteSequential(testRunAction); break;
                case ExecutionType.Parallel:
                    await ExecuteParallel(testRunAction); break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private async Task ExecuteSequential(Func<Task> testRunAction)
        {
            for(int i = 1; i <= configuration.Count; i++)
            {
                await testRunAction.Invoke().ConfigureAwait(false);
            }
        }

        private async Task ExecuteParallel(Func<Task> testRunAction)
        {
            var tasks = new List<Task>();

            for (int i = 1; i <= configuration.Count; i++)
            {
                tasks.Add(testRunAction.Invoke());
            }

            await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
        }
    }
}