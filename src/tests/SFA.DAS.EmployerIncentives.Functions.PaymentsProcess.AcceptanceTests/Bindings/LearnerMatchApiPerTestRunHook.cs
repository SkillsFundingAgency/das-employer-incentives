using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class LearnerMatchApiPerTestRunHook
    {
        [BeforeTestRun(Order = 2)]
        public static void InitialiseLearnerMatchApi(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.LearnerMatchApi = new TestLearnerMatchApi();
            stopwatch.Stop();
            Console.WriteLine($"[{nameof(LearnerMatchApiPerTestRunHook)}] time it took to spin up LearnerMatchApi: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterTestRun()]
        public static void CleanUpLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi?.Dispose();
        }
    }
}
