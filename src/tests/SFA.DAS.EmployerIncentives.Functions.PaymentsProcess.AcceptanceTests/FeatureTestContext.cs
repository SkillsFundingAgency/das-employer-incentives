namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
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
