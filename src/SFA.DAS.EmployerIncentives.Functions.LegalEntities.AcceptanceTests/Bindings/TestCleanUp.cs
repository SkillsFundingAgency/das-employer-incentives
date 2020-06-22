using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        private readonly TestContext _context;
        public TestCleanUp(TestContext context)
        {
            _context = context;
        }

        [AfterScenario()]
        public async Task CleanUpMessageBus()
        {
            if (_context.TestMessageBus.IsRunning)
            {
                await _context.TestMessageBus.Stop();
            }
            SqlHelper.DeleteTestDatabase(_context.DatabaseProperties);
            Directory.Delete(_context.TestDirectory.FullName, true);
            _context.FunctionsHost.Dispose();
        }
    }
}
