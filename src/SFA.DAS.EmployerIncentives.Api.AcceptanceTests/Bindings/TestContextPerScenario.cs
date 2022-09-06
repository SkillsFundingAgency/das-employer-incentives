using System;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public class TestContextPerScenario
    {
        private TestContext _testContext;

        [BeforeScenario(Order = 0)]
        public void CreateDatabase(TestContext context, ScenarioInfo scenarioInfo)
        {
            context.Initialise(TestRunContext.SqlServerImageInfo, scenarioInfo.Title);
            _testContext = context;

            Console.WriteLine($"TESTRUN: InstanceId = {_testContext.InstanceId}, TestName = '{_testContext.TestName}', TestDirectory = '{_testContext.TestDirectory}'");

        }

        [AfterScenario(Order = 1000)]
        public void TearDownDatabase()
        {
            //_testContext.Dispose();
        }
    }
}
