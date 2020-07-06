using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityCreated")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"the legal entity is not valid for Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            _testAccountTable.LegalEntityName = "";
        }

        [When(@"the legal entity is added to an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            var body = new AddLegalEntityRequest { 
                AccountLegalEntityId = _testAccountTable.AccountLegalEntityId, 
                LegalEntityId = _testAccountTable.LegalEntityId, 
                OrganisationName = _testAccountTable.LegalEntityName
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            await _testContext.WaitForHandler(async () => await _testContext.ApiClient.PostAsync($"/accounts/{_testAccountTable.Id}/legalEntities", content));
        }
    }
}
