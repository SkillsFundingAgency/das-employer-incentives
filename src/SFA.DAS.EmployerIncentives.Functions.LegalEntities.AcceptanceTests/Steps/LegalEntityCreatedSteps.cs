﻿using SFA.DAS.EmployerAccounts.Messages.Events;
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
        private readonly AccountTable _testAccountTable;

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = _testContext.TestData.GetOrCreate<AccountTable>();
        }

        [Given(@"the legal entity is not valid for Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            _testAccountTable.LegalEntityName = "";
        }

        [When(@"the legal entity is added to an account")]
        public async Task WhenAddedLegalEntityEventIsTriggered()
        {
            var message = new AddedLegalEntityEvent
            {
                AccountId = _testAccountTable.Id,
                AccountLegalEntityId = _testAccountTable.AccountLegalEntityId,
                LegalEntityId = _testAccountTable.LegalEntityId,
                OrganisationName = _testAccountTable.LegalEntityName
            };

            await _testContext.WaitForHandler(async () => await _testContext.TestMessageBus.Publish(message));
        }
    }
}
