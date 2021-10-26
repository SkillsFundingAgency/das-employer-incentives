using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Account = SFA.DAS.EmployerIncentives.Data.Models.Account;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityCreated")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly Account _account;
        private HttpStatusCode _expectedResult = HttpStatusCode.Created;
        private HttpResponseMessage _response;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"the legal entity is not valid for Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            _account.LegalEntityName = "";
            _expectedResult = HttpStatusCode.BadRequest;
        }

        [Given(@"the legal entity is already available in Employer Incentives")]
        public async Task GivenTheLegalEntityIsAlreadyAvailableInEmployerIncentives()
        {
            await DataAccess.SetupAccount(_account);
        }

        [When(@"the legal entity is added to an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            _response = await EmployerIncentiveApi.Put(
                    $"/accounts/{_account.Id}/legalEntities",
                    new AddLegalEntityRequest
                    {
                        AccountLegalEntityId = _account.AccountLegalEntityId,
                        LegalEntityId = _account.LegalEntityId,
                        OrganisationName = _account.LegalEntityName
                    });

            _response.StatusCode.Should().Be(_expectedResult);
        }
    }
}
