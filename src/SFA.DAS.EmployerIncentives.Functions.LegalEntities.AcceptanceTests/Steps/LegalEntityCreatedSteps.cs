using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Data.Tables;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityCreated")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccount;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccount = _testContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"the legal entity is not valid for Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            _testAccount.LegalEntityName = "";
        }

        [When(@"the legal entity is added to an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            var message = new AddedLegalEntityEvent
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
