using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "learnerMatchApi")]
    public class LearnerMatchApi
    {
        [BeforeScenario(Order = 5)]
        public void InitialiseLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi = new TestLearnerMatchApi();
        }

        [AfterScenario(Order = 5)]
        public void CleanUpLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi.Dispose();
        }
    }
}
