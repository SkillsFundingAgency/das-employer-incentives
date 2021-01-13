using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityDeleted")]
    public class LegalEntityDeletedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;

        public LegalEntityDeletedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>();
        }

        [When(@"a legal entity is removed from an account")]
        public async Task WhenALegalEntityIsRemovedFromAnAccount()
        {
            await EmployerIncentiveApi.Delete($"/accounts/{_testAccountTable.Id}/legalEntities/{_testAccountTable.AccountLegalEntityId}");
            
            EmployerIncentiveApi.GetLastResponse().StatusCode.Should().Be(HttpStatusCode.NoContent);
        }     
    }
}
