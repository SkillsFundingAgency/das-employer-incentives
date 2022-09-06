using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using SFA.DAS.EmployerIncentives.TestHelpers.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
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
            Console.WriteLine($@"[{nameof(DatabasePerTestRunHook)}] time it took to update `model` database: {stopwatch.Elapsed.Seconds} seconds");
            Console.WriteLine($"Sql server created at ServerName : {TestRunContext.SqlServerImageInfo.ServerName}, Port {TestRunContext.SqlServerImageInfo.Port}");
        }

        [AfterTestRun(Order = 100)]
        public static void Dispose()
        {
            _sqlImage?.Dispose();
        }
    }
}
