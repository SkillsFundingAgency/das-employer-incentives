using SFA.DAS.EmployerIncentives.TestHelpers.Services;
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
            Create(context);
            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario(Order = 100)]
        public void TearDownDatabase(TestContext context)
        {
            context.SqlDatabase?.Dispose();
        }

        private static void Create(TestContext context)
        {
            if (SqlServerImage.DockerIsRunning())
            {
                context.SqlDatabase = new SqlDatabase(TestRunContext.SqlServerImageInfo, context.InstanceId);
            }
            else
            {

                context.SqlDatabase = new Data.UnitTests.TestHelpers.SqlDatabase(context.InstanceId);
            }
        }
    }
}
