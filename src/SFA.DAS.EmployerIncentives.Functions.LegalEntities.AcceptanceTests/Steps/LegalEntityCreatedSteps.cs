using AutoFixture;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerIncentives.Data.Tables;
using System.Data.SqlClient;
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
            _testAccount = _testContext.Fixture.Create<Account>();
        }

        [Given(@"I have a legal entity that is not in the database")]
        public async Task GivenIHaveALegalEntityThatIsNotInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.DatabaseProperties.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id,  _testAccount.AccountLegalEntityId});

                account.Should().BeNullOrEmpty();
            }
        }

        [Given(@"I have a legal entity that is already in the database")]
        public async Task GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.DatabaseProperties.ConnectionString))
            {
                await dbConnection.InsertAsync(_testAccount);
            }
        }

        [Given(@"I have an invalid legal entity that is new")]
        public void GivenIHaveAnInvalidLegalEntityThatIsNew()
        {
            _testAccount.LegalEntityName = "";
        }

        [Given(@"I have a legal entity that is invalid")]
        public void GivenIHaveALegalEntityThatIsInvalid()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [When(@"added legal entity event is triggered")]
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

            await Task.Delay(1000); // TODO: hook into handler start  and completion
        }
        
        [Then(@"the legal entity should be available")]
        public async Task ThenTheLegalEntityShouldBeAvailable()
        {
            using (var dbConnection = new SqlConnection(_testContext.DatabaseProperties.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId});

                account.Should().HaveCount(1);
                account.Should().BeEquivalentTo(_testAccount);
            }
        }                

        [Then(@"the legal entity should not be available")]
        public async Task ThenTheLegalEntityShouldNotBeAvailable()
        {
            using (var dbConnection = new SqlConnection(_testContext.DatabaseProperties.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId });

                account.Should().HaveCount(0);
            }
        }
    }
}
