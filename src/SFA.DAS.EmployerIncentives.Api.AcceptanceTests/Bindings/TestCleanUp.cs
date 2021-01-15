using System;
using System.IO;
using System.Threading.Tasks;
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
        public async Task CleanUp()
        {
            try
            {
                Directory.Delete(_context.TestDirectory.FullName, true);
            }
            catch(Exception){}
        }
    }
}