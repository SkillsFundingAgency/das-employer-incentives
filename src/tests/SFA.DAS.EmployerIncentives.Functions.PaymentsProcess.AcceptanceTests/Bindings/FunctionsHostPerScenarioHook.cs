using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class FunctionsHostPerScenarioHook
    {
        [BeforeScenario(Order = 10)]
        public async Task InitialiseFunctions(TestContext context)
        {
            if (context.SqlDatabase?.DatabaseInfo != null == false)
            {
                throw new Exception("This hook requires a database");
            }

            if (context.LearnerMatchApi?.BaseAddress != null == false)
            {
                throw new Exception("This hook requires a Learner Match Api");
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            context.PaymentsProcessFunctions = new TestPaymentsProcessFunctions(context);

            await context.PaymentsProcessFunctions.Start();

            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(FunctionsHostPerScenarioHook)}] time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario]
        public void TearDownFunctions(TestContext context)
        {
            context.PaymentsProcessFunctions?.Dispose();
        }
    }
}