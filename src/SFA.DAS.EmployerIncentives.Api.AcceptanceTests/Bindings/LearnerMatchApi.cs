using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "learnerMatchApi")]
    public class LearnerMatchApi
    {
        private readonly AcceptanceTests.TestContext _context;

        public LearnerMatchApi(AcceptanceTests.TestContext context)
        {
            _context = context;            
        }

        [BeforeScenario(Order = 5)]
        public void InitialiseLearnerMatchApi()
        {
            _context.LearnerMatchApi = new TestLearnerMatchApi();
        }

        [AfterScenario()]
        public void CleanUpLearnerMatchApi()
        {
            _context.LearnerMatchApi.Dispose();
        }
    }
}
