using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly EmployerIncentiveApi EmployerIncentiveApi;

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            EmployerIncentiveApi = testContext.EmployerIncentiveApi;
        }        
    }
}
