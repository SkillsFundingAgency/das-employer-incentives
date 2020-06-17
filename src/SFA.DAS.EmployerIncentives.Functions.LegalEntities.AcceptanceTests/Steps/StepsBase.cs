using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    public class StepsBase
    {
        private readonly TestContext _testContext;

        public StepsBase(TestContext testContext)
        {
            _testContext = testContext;
        }        
        
        [BeforeScenario()]
        public void InitialiseTestDatabaseData()
        {           
        }
    }
}
