using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntitiesForAccountRequested")]
    public class LegalEntitiesForAccountRequestedSteps : StepsBase
    {
        private IEnumerable<LegalEntityDto> _getLegalEntitiesResponse;
        private readonly Account _testAccountTable;

        public LegalEntitiesForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testAccountTable = testContext.TestData.GetOrCreate<Account>();
        }

        [When(@"a client requests the legal entities for the account")]
        public async Task WhenAClientRequestsTheLegalEntitiesForTheAccount()
        {
            var url = $"/accounts/{_testAccountTable.Id}/LegalEntities";
            var (status, data) = 
                await EmployerIncentiveApi.Client.GetValueAsync<IEnumerable<LegalEntityDto>>(url);
            
            status.Should().Be(HttpStatusCode.OK);

            _getLegalEntitiesResponse = data;
        }

        [Then(@"the legal entities are returned")]
        public void ThenTheLegalEntitiesAreReturned()
        {
            _getLegalEntitiesResponse.Should().NotBeEmpty();
            _getLegalEntitiesResponse.First().AccountLegalEntityId.Should()
                .Be(_testAccountTable.AccountLegalEntityId);
        }

    }
}
