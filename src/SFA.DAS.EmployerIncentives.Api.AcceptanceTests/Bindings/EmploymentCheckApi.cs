using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "employmentCheckApi")]
    public class EmploymentCheckApi
    {
        private readonly AcceptanceTests.TestContext _context;

        public EmploymentCheckApi(AcceptanceTests.TestContext context)
        {
            _context = context;            
        }

        [BeforeScenario(Order = 5)]
        public void InitialiseEmploymentCheckApi()
        {
            _context.EmploymentCheckApi = new TestEmploymentCheckApi();
        }

        [AfterScenario()]
        public void CleanUpEmploymentCheckApi()
        {
            _context.EmploymentCheckApi.Dispose();
        }
    }
}
