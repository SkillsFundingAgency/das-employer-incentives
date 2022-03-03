namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public static class FeatureTestContext
    {
        public static TestData FeatureData { get; set; }

        static FeatureTestContext()
        {
            FeatureData = new TestData();
        }
    }
}
