using SFA.DAS.EmployerIncentives.TestHelpers.Services;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.TestFramework;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class DatabasePerScenarioHook
    {
        [BeforeScenario(Order = 2)]
        public void CreateDatabase(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();            
            context.SqlDatabase = new SqlDatabase(TestRunContext.SqlServerImageInfo, context.InstanceId);
            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario(Order = 100)]
        public void TearDownDatabase(TestContext context)
        {
            context.SqlDatabase?.Dispose();
        }
    }
}
