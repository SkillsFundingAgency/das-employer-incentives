using System.IO;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
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
            _context.EmployerIncentiveApi.Dispose();
            _context.SqlDatabase.Dispose();
            Directory.Delete(_context.TestDirectory.FullName, true);
        }
    }
}
