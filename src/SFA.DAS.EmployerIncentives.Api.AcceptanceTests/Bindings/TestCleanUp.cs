using System;
using System.IO;
using System.Threading;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        private readonly TestContext _context;
        private readonly CancellationTokenSource _tokenSource;

        public TestCleanUp(TestContext context)
        {         
            _context = context;
            _tokenSource = new CancellationTokenSource();
        }

        [BeforeScenario(Order = 1)]
        public void StartUp()
        {
            _context.CancellationToken = _tokenSource.Token;
        }

        [AfterScenario(Order = 1)]
        public void Cancel()
        {
            _tokenSource.Cancel();
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