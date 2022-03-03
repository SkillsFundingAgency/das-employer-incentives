using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "database")]
    public class DatabasePerScenarioHook
    {
        [BeforeScenario(Order = 2)]
        public async Task CreateDatabase(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.SqlDatabase = await new SqlDatabase(context.InstanceId).Create();
            stopwatch.Stop();
            Console.WriteLine($"[{nameof(DatabasePerScenarioHook)}] time it took to deploy test database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario(Order = 9)]
        public static void TearDownDatabase(TestContext context)
        {            
            context.SqlDatabase?.Dispose();
        }
    }
}
