using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    //  [Scope(Tag = "database")]
    public class DatabasePerScenarioHook
    {
        [BeforeScenario(Order = 1)]
        public void InitialiseDatabase(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.SqlDatabase = new SqlDatabase2();
            stopwatch.Stop();
            Console.WriteLine($"[{nameof(DatabasePerScenarioHook)}] time it took to deploy database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [BeforeScenario]
        public static void TearDownDatabase(TestContext context)
        {
            // context.SqlDatabase?.Dispose();
        }
    }
}
