using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class CustomerEngagementPaymentApi
    {
        private readonly TestContext _context;
        private readonly FeatureInfo _featureInfo;

        public CustomerEngagementPaymentApi(TestContext context, FeatureInfo featureInfo)
        {
            _context = context;
            _featureInfo = featureInfo;
        }

        [BeforeScenario(Order = 1)]
        public void Initialise()
        {
            if (_context.PaymentsApi == null)
            {
                _context.PaymentsApi = FeatureTestContext.FeatureData.Get<MockApi>(_featureInfo.Title + nameof(PaymentsApi));
            }
        }

        [AfterScenario()]
        public void CleanUpPaymentsApi()
        {
            _context.PaymentsApi.Reset();
        }
    }
}
