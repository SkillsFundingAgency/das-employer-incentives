using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "learnerMatchApi")]
    public class LearnerMatchApi
    {
        private readonly TestContext _context;

        public LearnerMatchApi(TestContext context)
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
            _context.LearnerMatchApi?.Dispose();
        }
    }
}
