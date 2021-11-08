using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityUpdated")]
    public class LegalEntityUpdatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;
        private HttpResponseMessage _response;
        private Fixture _fixture;
        private string _newLegalEntityName;

        public LegalEntityUpdatedSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"the legal entity is already available in Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsAlreadyExists()
        {
            _response = await EmployerIncentiveApi.Put(
                $"/accounts/{_testAccountTable.Id}/legalEntities",
                new AddLegalEntityRequest
                {
                    AccountLegalEntityId = _testAccountTable.AccountLegalEntityId,
                    LegalEntityId = _testAccountTable.LegalEntityId,
                    OrganisationName = _testAccountTable.LegalEntityName
                });

            _response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [When(@"the legal entity name is amended in an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            _newLegalEntityName = _fixture.Create<string>();

            _response = await EmployerIncentiveApi.Put(
                    $"/accounts/{_testAccountTable.Id}/legalEntities",
                    new AddLegalEntityRequest
                    {
                        AccountLegalEntityId = _testAccountTable.AccountLegalEntityId,
                        LegalEntityId = _testAccountTable.LegalEntityId,
                        OrganisationName = _newLegalEntityName
                    });

            _response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Then(@"the legal entity should be updated with the latest name")]
        public void ThenTheLegalEntityShouldBeUpdatedWithTheLatestDetails()
        {
            var account = DataAccess.GetAccountByAccountLegalEntityId(_testAccountTable.Id, _testAccountTable.AccountLegalEntityId);

            account.LegalEntityName.Should().Be(_newLegalEntityName);
        }
    }
}
