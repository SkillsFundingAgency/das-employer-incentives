using SFA.DAS.EmployerIncentives.TestHelpers.Services;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "database")]
    public class DatabasePerScenarioHook
    {
        [BeforeScenario(Order = 2)]
        public void CreateDatabase(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.SqlDatabase = new SqlDatabase(TestRunContext.SqlServerImageInfo, context.InstanceId);
            stopwatch.Stop();
            Console.WriteLine($"TESTRUN: SqlDataSource = {context.SqlDataSource}, DatabaseName = {context.SqlDatabase.DatabaseInfo.DatabaseName}");
            Console.WriteLine($"TESTRUN: [{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario]
        public static void TearDownDatabase(TestContext context)
        {
            Console.WriteLine($"TESTRUN: SqlDatabase Dispose");
            context.SqlDatabase?.Dispose();
        }
    }
}
