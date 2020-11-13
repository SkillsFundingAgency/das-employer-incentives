using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class TestCleanUp
    {
        [BeforeScenario]
        public async Task CheckFunctionsArentRunning(TestContext context)
        {
            // await context.PaymentsProcessFunctions.AllFunctionOrchestrationCompleted();
        }

        [AfterScenario()]
        public async Task CleanUp(TestContext context)
        {
            // await context.PaymentsProcessFunctions.AllFunctionOrchestrationCompleted();
            context.SqlDatabase.ClearDown();
        }

    }
}
