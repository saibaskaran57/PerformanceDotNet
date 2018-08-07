namespace PerformanceDotNet.Models
{
    internal sealed class TestResult
    {
        public double SetupDuration { get; private set; }

        public double TestDuration { get; private set; }

        public double TearDownDuration { get; private set; }

        public double OverallDuration
        {
            get
            {
                return SetupDuration + TestDuration + TearDownDuration;
            }
        }

        public void CollectSetupDuration(double duration)
        {
            SetupDuration += duration;
        }

        public void CollectTestDuration(double duration)
        {
            TestDuration += duration;
        }

        public void CollectTearDownDuration(double duration)
        {
            TearDownDuration += duration;
        }
    }
}