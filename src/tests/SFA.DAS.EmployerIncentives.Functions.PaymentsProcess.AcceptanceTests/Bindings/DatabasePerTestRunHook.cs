//using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
//using System;
//using System.Diagnostics;
//using TechTalk.SpecFlow;

//namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
//{
//    [Binding]
//    public static class DatabasePerTestRunHook
//    {
//        [BeforeTestRun(Order = 1)]
//        public static void InitialiseDatabase(TestContext context)
//        {
//            var stopwatch = new Stopwatch();
//            stopwatch.Start();
//            context.SqlDatabase = new SqlDatabase();
//            stopwatch.Stop();
//            Console.WriteLine($"[{nameof(DatabasePerTestRunHook)}] time it took to deploy database: {stopwatch.Elapsed.Seconds} seconds");
//        }

//        [AfterTestRun()]
//        public static void TearDownDatabase(TestContext context)
//        {
//            context.SqlDatabase?.Dispose();
//        }
//    }
//}
