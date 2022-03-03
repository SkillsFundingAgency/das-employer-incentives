using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "accountApi")]
    public class AccountApi
    {
        [BeforeFeature()]
        public static void InitialiseAccountApi(FeatureInfo featureInfo)
        {
            lock (FeatureTestContext.FeatureData)
            {
                FeatureTestContext.FeatureData.GetOrCreate(featureInfo.Title + nameof(TestAccountApi), () =>
                {
                    return new TestAccountApi();
                });
            }
        }

        [AfterFeature()]
        public static void CleanUpAccountApi(FeatureInfo featureInfo)
        {
            var accountApi = FeatureTestContext.FeatureData.Get<TestAccountApi>(featureInfo.Title + nameof(TestAccountApi));
            if (accountApi != null)
            {
                accountApi.Dispose();
            }
        }

        [BeforeScenario(Order = 1)]
        public void Initialise(TestContext context, FeatureInfo featureInfo)
        {
            if (context.AccountApi == null)
            {
                context.AccountApi = FeatureTestContext.FeatureData.Get<TestAccountApi>(featureInfo.Title + nameof(TestAccountApi));
            }
        }

        [AfterScenario(Order = 1)]
        public void CleanUpPaymentsApi(TestContext context)
        {
            context.AccountApi.Reset();
        }
    }
}
