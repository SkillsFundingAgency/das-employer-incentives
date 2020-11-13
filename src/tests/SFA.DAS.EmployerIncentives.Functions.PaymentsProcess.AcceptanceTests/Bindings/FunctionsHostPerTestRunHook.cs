//using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;
//using TechTalk.SpecFlow;

//namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
//{
//    [Binding]

//    public static class FunctionsHostPerTestRunHook
//    {
//        [BeforeTestRun(Order = 10)]
//        public static async Task InitialiseFunctions(TestContext context)
//        {
//            if (context.SqlDatabase?.DatabaseInfo != null == false)
//            {
//                throw new Exception("This hook requires a database");
//            }

//            if (context.LearnerMatchApi?.BaseAddress != null == false)
//            {
//                throw new Exception("This hook requires a Learner Match Api");
//            }

//            var stopwatch = new Stopwatch();
//            stopwatch.Start();

//            context.PaymentsProcessFunctions = new TestPaymentsProcessFunctions(context.SqlDatabase.DatabaseInfo.ConnectionString,
//                context.LearnerMatchApi.BaseAddress);

//            await context.PaymentsProcessFunctions.Start();

//            stopwatch.Stop();
//            Console.WriteLine($"[{nameof(FunctionsHostPerTestRunHook)}] time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Seconds} seconds");
//        }

//        [AfterTestRun()]
//        public static void TearDownFunctions(TestContext context)
//        {
//            context.PaymentsProcessFunctions?.Dispose();
//        }
//    }
//}