using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class DatabasePerTestRunHook
    {
        [BeforeTestRun(Order = 1)]
        public static void InitialiseDatabase(TestContext context)
        {
            context.SqlDatabase = new SqlDatabase();
        }

        [AfterTestRun()]
        public static void TearDownDatabase(TestContext context)
        {
            context.SqlDatabase?.Dispose();
        }
    }
}
