using Microsoft.AspNetCore.Mvc.Testing;
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

        [BeforeScenario(Order = 5)]
        public void InitialiseApi()
        {
            var eventsHook = new Hook<object>();
            _context.Hooks.Add(eventsHook);

            var commandsHook = new Hook<ICommand>();
            _context.Hooks.Add(commandsHook);

            var webApi = new TestWebApi(_context, eventsHook, commandsHook);
            var options = new WebApplicationFactoryClientOptions
            {
                BaseAddress = new System.Uri(@"https://localhost:5001")
            };
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient(options));
        }
    }
}
