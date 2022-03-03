using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityRequested")]
    public class LegalEntityRequestedSteps : StepsBase
    {
        private LegalEntityDto _getLegalEntityResponse;
        private readonly Account _testAccountTable;

        public LegalEntityRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testAccountTable = testContext.TestData.GetOrCreate<Account>();
        }

        [When(@"a client requests the legal entity")]
        public async Task WhenAClientRequestsTheLegalEntity()
        {
            var url = $"/accounts/{_testAccountTable.Id}/LegalEntities/{_testAccountTable.AccountLegalEntityId}";
            var (status, data) = 
                await EmployerIncentiveApi.Client.GetValueAsync<LegalEntityDto>(url, TestContext.CancellationToken);
            
            status.Should().Be(HttpStatusCode.OK);

            _getLegalEntityResponse = data;
        }

        [Then(@"the legal entity is returned")]
        public void ThenTheLegalEntityIsReturned()
        {
            _getLegalEntityResponse.Should().NotBeNull();
            _getLegalEntityResponse.AccountLegalEntityId.Should().Be(_testAccountTable.AccountLegalEntityId);
        }

    }
}
