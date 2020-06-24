using AutoFixture;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Data.Tables;
using SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityCreated")]
    public class LegalEntityCreatedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccount;
        private bool hasCompleted = false;
        private bool hasTimedOut = false;        

        public LegalEntityCreatedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccount = _testContext.Fixture.Create<Account>();
            _testContext.CommandHandlerHooks = new CommandHandlerHooks
            {
                OnHandlerEnd = (command) => { hasCompleted = true; }
            };
        }

        [Given(@"the legal entity is not stored in Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsNotInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id,  _testAccount.AccountLegalEntityId});

                account.Should().BeNullOrEmpty();
            }
        }

        [Given(@"the legal entity is already stored in Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_testAccount);
            }
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

            await  _testContext.TestMessageBus.Publish(message);
            await WaitForHandlerCompletion();
        }

        [Then(@"the legal entity should be stored in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldBeAvailable()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId});

                account.Should().HaveCount(1);
                account.Should().BeEquivalentTo(_testAccount);
            }
        }                

        [Then(@"the legal entity should not be stored in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldNotBeAvailable()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId });

                account.Should().HaveCount(0);
            }
        }

        private void HasTimedOut(object state)
        {
            hasTimedOut = true;
        }
        private async Task WaitForHandlerCompletion()
        {
            hasTimedOut = false;
            using (Timer timer = new Timer(new TimerCallback(HasTimedOut), null, 3000, Timeout.Infinite))
            {
                while (!hasCompleted && !hasTimedOut)
                {
                    await Task.Delay(100);
                }
            }
            if (hasTimedOut)
            {
                Assert.Fail("Test timed out waiting for handler to complete");
            }
        }
    }
}
