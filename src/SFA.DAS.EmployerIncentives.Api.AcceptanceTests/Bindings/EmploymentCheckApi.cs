using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "employmentCheckApi")]
    public class EmploymentCheckApi
    {
        [BeforeFeature()]
        public static void InitialiseEmploymentCheckApi(FeatureInfo featureInfo)
        {
            lock (FeatureTestContext.FeatureData)
            {
                FeatureTestContext.FeatureData.GetOrCreate(featureInfo.Title + nameof(TestEmploymentCheckApi), () =>
                {
                    return new TestEmploymentCheckApi();
                });
            }
        }

        [AfterFeature()]
        public static void CleanUpEmploymentCheckApi(FeatureInfo featureInfo)
        {
            var employmentCheckApi = FeatureTestContext.FeatureData.Get<TestEmploymentCheckApi>(featureInfo.Title + nameof(TestEmploymentCheckApi));
            if (employmentCheckApi != null)
            {
                employmentCheckApi.Dispose();
            }
        }

        [BeforeScenario(Order = 6)]
        public void Initialise(TestContext context, FeatureInfo featureInfo)
        {
            if (context.EmploymentCheckApi == null)
            {
                context.EmploymentCheckApi = FeatureTestContext.FeatureData.Get<TestEmploymentCheckApi>(featureInfo.Title + nameof(TestEmploymentCheckApi));
            }
        }

        [AfterScenario(Order = 6)]
        public void CleanUpPaymentsApi(TestContext context)
        {
            context.EmploymentCheckApi.Reset();
        }
    }
}
