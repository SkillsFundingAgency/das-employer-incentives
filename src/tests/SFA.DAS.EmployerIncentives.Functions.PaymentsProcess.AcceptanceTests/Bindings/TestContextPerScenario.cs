using System;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class TestContextPerScenario
    {
        private TestContext _testContext;

        [BeforeScenario(Order = 0)]
        public void CreateDatabase(TestContext context)
        {
            context.Initialise(TestRunContext.SqlServerImageInfo);
            _testContext = context;
        }

        [AfterScenario(Order = 1000)]
        public void TearDownDatabase()
        {
            _testContext.Dispose();
        }
    }
}
