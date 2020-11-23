using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Diagnostics;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class LearnerMatchApiPerScenarioHook
    {
        [BeforeScenario(Order = 3)]
        public void InitialiseLearnerMatchApi(TestContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            context.LearnerMatchApi = new TestLearnerMatchApi();
            stopwatch.Stop();
            Console.WriteLine($@"[{nameof(LearnerMatchApiPerScenarioHook)}] time it took to spin up LearnerMatchApi: {stopwatch.Elapsed.Seconds} seconds");
        }

        [AfterScenario]
        public void CleanUpLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi?.Dispose();
        }
    }
}
