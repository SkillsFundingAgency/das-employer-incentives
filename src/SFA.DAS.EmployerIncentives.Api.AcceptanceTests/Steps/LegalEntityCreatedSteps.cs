using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityCreated")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;
        private HttpStatusCode _expectedResult = HttpStatusCode.Created;
        private HttpResponseMessage _response;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"the legal entity is not valid for Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            _testAccountTable.LegalEntityName = "";
            _expectedResult = HttpStatusCode.BadRequest;
        }

        [When(@"the legal entity is added to an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            _response = await EmployerIncentiveApi.Put(
                    $"/accounts/{_testAccountTable.Id}/legalEntities",
                    new AddLegalEntityRequest
                    {
                        AccountLegalEntityId = _testAccountTable.AccountLegalEntityId,
                        LegalEntityId = _testAccountTable.LegalEntityId,
                        OrganisationName = _testAccountTable.LegalEntityName
                    });

            _response.StatusCode.Should().Be(_expectedResult);
        }
    }
}
