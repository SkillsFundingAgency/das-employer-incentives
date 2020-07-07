using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntitiesForAccountRequested")]
    public class LegalEntitiesForAccountRequestedSteps : StepsBase
    {
        private GetLegalEntitiesResponse _getLegalEntitiesResponse;
        private long _accountId;

        public LegalEntitiesForAccountRequestedSteps(TestContext testContext) : base(testContext) { }

        [Given(@"an account with legal entities is in employer incentives")]
        public async Task GivenAnAccountWithLegalEntitiesIsInEmployerIncentives()
        {
            var account = TestContext.TestData.GetOrCreate<Account>();
            var request = account.ToAddLegalEntityRequest();
            _accountId = account.Id;
            await EmployerIncentiveApi.Post($"/accounts/{account.Id}/legalEntities", request);

            account = TestContext.TestData.GetOrCreate<Account>();
            request = account.ToAddLegalEntityRequest();

            await EmployerIncentiveApi.Post($"/accounts/{account.Id}/legalEntities", request);
        }

        [When(@"a client requests the legal entities for the account")]
        public async Task WhenAClientRequestsTheLegalEntitiesForTheAccount()
        {
            var (status, data) = await EmployerIncentiveApi.Client.GetValueAsync<GetLegalEntitiesResponse>($"/accounts/{_accountId}/LegalEntities");
            
            status.Should().Be(HttpStatusCode.OK);

            _getLegalEntitiesResponse = data;
        }

        [Then(@"the legal entities are returned")]
        public void ThenTheLegalEntitiesAreReturned()
        {
            _getLegalEntitiesResponse.LegalEntities.Should().NotBeEmpty();
        }

    }
}
