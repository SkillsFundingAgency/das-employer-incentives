using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly EmployerIncentiveApi EmployerIncentiveApi;
        protected readonly Fixture Fixture;

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            EmployerIncentiveApi = testContext.EmployerIncentiveApi;
            Fixture = new Fixture();

            var hook = testContext.Hooks.SingleOrDefault(h => h is Hook<object>) as Hook<object>;

            if (hook != null)
            {
                hook.OnProcessed = (message) =>
                {
                    testContext.EventsPublished.Add(message);
                };
            }
        }
    }
}
