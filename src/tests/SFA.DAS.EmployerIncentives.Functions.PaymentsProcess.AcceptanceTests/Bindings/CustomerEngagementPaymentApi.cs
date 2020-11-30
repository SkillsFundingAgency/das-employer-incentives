using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class CustomerEngagementPaymentApi
    {
        private readonly TestContext _context;

        public CustomerEngagementPaymentApi(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 1)]
        public void Initialise()
        {
            if (_context.PaymentsApi == null)
                _context.PaymentsApi = new MockApi();
        }
    }
}
