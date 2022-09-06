using SFA.DAS.EmployerIncentives.TestHelpers.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    public static class DatabasePerTestRunHook
    {
        private static SqlServerImage _sqlImage;

        [BeforeTestRun(Order = 1)]
        public static async Task RefreshDatabaseModel()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _sqlImage = await SqlServerImage.Create();
            TestRunContext.SqlServerImageInfo = _sqlImage.SqlServerImageInfo;
            stopwatch.Stop();            
            Console.WriteLine($"[{nameof(DatabasePerTestRunHook)}] time it took to update `model` database: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterTestRun(Order = 100)]
        public static void Dispose()
        {
            Console.WriteLine($"TESTRUN: SqlImage Dispose");
            _sqlImage?.Dispose();
        }
    }
}
