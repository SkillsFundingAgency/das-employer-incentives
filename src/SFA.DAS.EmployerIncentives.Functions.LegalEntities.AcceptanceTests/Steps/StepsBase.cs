using System.Net.Http;
using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly HttpClient HttpClient;

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            HttpClient = TestContext.ApiClient;
        }        
    }
}
