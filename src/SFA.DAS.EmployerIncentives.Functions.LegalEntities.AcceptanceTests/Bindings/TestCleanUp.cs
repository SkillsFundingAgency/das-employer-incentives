using System.IO;
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
        public void CleanUp()
        {
            //if (_context.TestMessageBus.IsRunning)
            //{
            //    await _context.TestMessageBus.Stop();
            //}
            _context.ApiClient.Dispose();
            _context.SqlDatabase.Dispose();
            Directory.Delete(_context.TestDirectory.FullName, true);
            //_context.FunctionsHost.Dispose();
        }
    }
}
