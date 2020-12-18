using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class LearnerMatchApi
    {
        private readonly TestContext _testContext;
        private readonly FeatureInfo _featureInfo;

        public LearnerMatchApi(TestContext testContext, FeatureInfo featureInfo)
        {
            _testContext = testContext;
            _featureInfo = featureInfo;
        }

        [BeforeFeature()]
        public static void InitialiseLearnerMatchApi(FeatureInfo featureInfo)
        {
            lock (FeatureTestContext.FeatureData)
            {
                FeatureTestContext.FeatureData.GetOrCreate(featureInfo.Title + nameof(LearnerMatchApi), () =>
                {
                    return new MockApi();
                });
            }
        }

        [AfterFeature()]
        public static void CleanUpLearnerMatchApi(FeatureInfo featureInfo)
        {
            var learnerMatchApi = FeatureTestContext.FeatureData.Get<MockApi>(featureInfo.Title + nameof(LearnerMatchApi));
            if (learnerMatchApi != null)
            {
                learnerMatchApi.Dispose();
            }
        }

        [BeforeScenario(Order = 3)]
        public void InitialiseLearnerMatchApi()
        {
            if (_testContext.LearnerMatchApi == null)
            {
                _testContext.LearnerMatchApi = FeatureTestContext.FeatureData.Get<MockApi>(_featureInfo.Title + nameof(LearnerMatchApi));
            }
        }

        [AfterScenario()]
        public void CleanUpLearnerMatchApi()
        {
            _testContext.LearnerMatchApi.Reset();
        }
    }
}
