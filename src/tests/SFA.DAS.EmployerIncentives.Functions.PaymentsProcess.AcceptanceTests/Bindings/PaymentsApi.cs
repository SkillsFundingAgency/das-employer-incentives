using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class PaymentsApi
    {                   
        [BeforeFeature()]
        public static void InitialisePaymentsApi(FeatureInfo featureInfo)
        {
            lock (FeatureTestContext.FeatureData)
            {
                FeatureTestContext.FeatureData.GetOrCreate(featureInfo.Title + nameof(PaymentsApi), () =>
                {
                    return new MockApi();
                });
            }
        }

        [AfterFeature()]
        public static void CleanUpPaymentsApi(FeatureInfo featureInfo)
        {
            var paymentsApi = FeatureTestContext.FeatureData.Get<MockApi>(featureInfo.Title + nameof(PaymentsApi));
            if (paymentsApi != null)
            {
                paymentsApi.Dispose();
            }
        }
    }
}
