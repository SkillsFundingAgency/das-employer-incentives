using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        [AfterScenario()]
        public void CleanUp(TestContext context)
        {
            context.SqlDatabase.ClearDown();
        }
    }
}
