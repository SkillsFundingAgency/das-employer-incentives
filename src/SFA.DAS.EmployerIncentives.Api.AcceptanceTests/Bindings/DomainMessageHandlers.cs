using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "domainMessageHandlers")]
    public class DomainMessageHandlers
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _hostConfig;
        private readonly Dictionary<string, string> _appConfig;

        public DomainMessageHandlers(TestContext context)
        {
            _context = context;
            _hostConfig = new Dictionary<string, string>();
            _appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" }
            };
        }

        [BeforeScenario(Order = 6)]
        public async Task InitialiseFunctions()
        {
            _context.DomainMessageHandlers = new TestDomainMessageHandlers(_context);
            await _context.DomainMessageHandlers.Start();
        }
    }
}
