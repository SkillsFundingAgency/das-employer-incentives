using SFA.DAS.EmployerIncentives.Abstractions.Commands;
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

            var commandsHook = new Hook<ICommand>();
            _context.Hooks.Add(commandsHook);

            var webApi = new TestWebApi(_context, eventsHook, commandsHook);
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient());
        }
    }
}
