using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "api")]
    public class Api
    {
        private readonly TestContext _context;

        public Api(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario()]
        public void InitialiseApi()
        {
            var webApi = new TestWebApi(_context);
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient());
        }
    }
}
