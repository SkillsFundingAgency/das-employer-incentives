using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class LearnerMatchApiPerTestRunHook
    {
        [BeforeTestRun(Order = 2)]
        public static void InitialiseLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi = new TestLearnerMatchApi();
        }

        [AfterTestRun()]
        public static void CleanUpLearnerMatchApi(TestContext context)
        {
            context.LearnerMatchApi?.Dispose();
        }
    }
}
