using SFA.DAS.EmployerAccounts.Messages.Events;
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
        private readonly Account _testAccount;

        public LegalEntityDeletedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccount = _testContext.TestData.GetOrCreate<Account>();
        }

        [When(@"a legal entity is removed from an account")]
        public async Task WhenALegalEntityIsRemovedFromAnAccount()
        {
            var message = new RemovedLegalEntityEvent
            {
                AccountId = _testAccount.Id,
                AccountLegalEntityId = _testAccount.AccountLegalEntityId,
                LegalEntityId = _testAccount.LegalEntityId,
                OrganisationName = _testAccount.LegalEntityName
            };

            await _testContext.WaitForHandler(async () => await _testContext.TestMessageBus.Publish(message));
        }     
    }
}
