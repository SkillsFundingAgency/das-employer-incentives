using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "accountApi")]
    public class AccountApi
    {
        private readonly TestContext _context;

        public AccountApi(TestContext context)
        {
            _context = context;            
        }

        [BeforeScenario(Order = 4)]
        public void InitialiseAccountApi()
        {
            _context.AccountApi = new TestAccountApi();
        }

        [AfterScenario()]
        public void CleanUpAccountApi()
        {
            _context.AccountApi.Dispose();
        }
    }
}
