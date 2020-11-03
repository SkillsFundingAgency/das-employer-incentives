using System;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        private TestContext _context;
        public TestCleanUp(TestContext context)
        {
            _context = context;
        }

        [AfterScenario()]
        public async Task CleanUp()
        {
            if (_context.MessageBus != null && _context.MessageBus.IsRunning)
            {
                await _context.MessageBus.Stop();
            }

            _context.EmployerIncentiveApi?.Dispose();
            _context.EmployerIncentivesWebApiFactory?.Dispose();
            _context.AccountApi?.Dispose();
            _context.DomainMessageHandlers?.Dispose();
            _context.SqlDatabase?.Dispose();

            _context = null;

            try
            {
                Directory.Delete(_context.TestDirectory.FullName, true);
            }
            catch(Exception){}
        }
    }
}