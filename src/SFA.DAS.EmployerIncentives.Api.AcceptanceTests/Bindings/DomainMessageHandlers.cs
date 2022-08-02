using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "domainMessageHandlers")]
    public class DomainMessageHandlers
    {
        private readonly TestContext _context;

        public DomainMessageHandlers(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 6)]
        public async Task InitialiseFunctions()
        {
            _context.DomainMessageHandlers = new TestDomainMessageHandlers(_context);
            await _context.DomainMessageHandlers.Start();
        }

        [AfterScenario(Order = 1)]
        public async Task CleanUp()
        {
            try
            {
                await _context.DomainMessageHandlers.Stop();
            }
            catch (OperationCanceledException) { }
            _context.DomainMessageHandlers?.Dispose();
        }
    }
}
