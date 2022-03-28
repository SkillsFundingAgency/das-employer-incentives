using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using TechTalk.SpecFlow;
using Account = SFA.DAS.EmployerIncentives.Data.Models.Account;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityRequested")]
    public class LegalEntityRequestedSteps : StepsBase
    {
        private LegalEntity _getLegalEntityResponse;
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
                await EmployerIncentiveApi.Client.GetValueAsync<LegalEntity>(url);
            
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
