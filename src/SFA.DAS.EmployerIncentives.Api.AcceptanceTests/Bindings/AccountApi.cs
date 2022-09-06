using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "accountApi")]
    public class AccountApi
    {
        private readonly AcceptanceTests.TestContext _context;

        public AccountApi(AcceptanceTests.TestContext context)
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
            _context.AccountApi?.Dispose();
        }
    }
}
