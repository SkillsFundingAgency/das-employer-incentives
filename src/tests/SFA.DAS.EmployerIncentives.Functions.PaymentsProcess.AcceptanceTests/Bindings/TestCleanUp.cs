using System;
using System.IO;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        private readonly TestContext _context;

        public TestCleanUp(TestContext context)
        {         
            _context = context;
        }

        [AfterScenario(Order = 100)]
        public void CleanUp()
        {
            try
            {
                Directory.Delete(_context.TestDirectory.FullName, true);
            }
            catch(Exception){}
        }
    }
}