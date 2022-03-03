using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "learnerMatchApi")]
    public class LearnerMatchApi
    {
        [BeforeFeature()]
        public static void InitialiseAccountApi(FeatureInfo featureInfo)
        {
            lock (FeatureTestContext.FeatureData)
            {
                FeatureTestContext.FeatureData.GetOrCreate(featureInfo.Title + nameof(TestLearnerMatchApi), () =>
                {
                    return new TestLearnerMatchApi();
                });
            }
        }

        [AfterFeature()]
        public static void CleanUpAccountApi(FeatureInfo featureInfo)
        {
            var learnerMatchApi = FeatureTestContext.FeatureData.Get<TestLearnerMatchApi>(featureInfo.Title + nameof(TestLearnerMatchApi));
            if (learnerMatchApi != null)
            {
                learnerMatchApi.Dispose();
            }
        }

        [BeforeScenario(Order = 1)]
        public void Initialise(TestContext context, FeatureInfo featureInfo)
        {
            if (context.LearnerMatchApi == null)
            {
                context.LearnerMatchApi = FeatureTestContext.FeatureData.Get<TestLearnerMatchApi>(featureInfo.Title + nameof(TestLearnerMatchApi));
            }
        }

        [AfterScenario(Order = 1)]
        public void CleanUpPaymentsApi(TestContext context)
        {
            context.LearnerMatchApi.Reset();
        }
    }
}

