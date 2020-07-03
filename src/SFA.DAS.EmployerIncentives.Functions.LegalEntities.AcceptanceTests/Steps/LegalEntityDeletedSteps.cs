using SFA.DAS.EmployerIncentives.Data.Tables;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityDeleted")]
    public class LegalEntityDeletedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly AccountTable _testAccountTable;

        public LegalEntityDeletedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<AccountTable>();
        }

        [When(@"a legal entity is removed from an account")]
        public async Task WhenALegalEntityIsRemovedFromAnAccount()
        {
            await _testContext.WaitForHandler(async () => await _testContext.ApiClient.DeleteAsync($"/accounts/{_testAccountTable.Id}/legalEntities/{_testAccountTable.AccountLegalEntityId}"));
        }     
    }
}
