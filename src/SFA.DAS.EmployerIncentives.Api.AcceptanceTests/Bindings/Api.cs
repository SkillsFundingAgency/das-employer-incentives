using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
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
            var eventsHook = new Hook<object>();
            _context.Hooks.Add(eventsHook);

            var webApi = new TestWebApi(_context, eventsHook);
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient());
        }
    }
}
