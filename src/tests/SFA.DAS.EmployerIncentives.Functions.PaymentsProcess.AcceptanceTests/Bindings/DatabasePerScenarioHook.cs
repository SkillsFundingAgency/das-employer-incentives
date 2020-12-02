using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

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
            context.SqlDatabase = new SqlDatabase();
            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario]
        public static void TearDownDatabase(TestContext context)
        {
            context.SqlDatabase?.Dispose();
        }
    }
}
