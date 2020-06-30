using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Tables;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Steps
{
    [Binding]
    public class LegalEntitySteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccount;

        public LegalEntitySteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccount = _testContext.TestData.GetOrCreate<Account>();
        }

        [Given(@"a legal entity is not available in employer incentives")]
        [Given(@"the legal entity is in not Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsNotInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId });

                account.Should().BeNullOrEmpty();
            }
        }

        [Given(@"a legal entity that is in employer incentives")]
        [Given(@"the legal entity is already available in Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_testAccount);
            }
        }

        [Then(@"the legal entity should no longer be available in employer incentives")]
        [Then(@"the legal entity is still not available in employer incentives")]
        [Then(@"the legal entity should not be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldNotBeAvailableInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId });

                account.Should().HaveCount(0);
            }
        }

        [Then(@"the legal entity should be available in Employer Incentives")]
        [Then(@"the legal entity should still be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldBeAvailableInTheDatabase()
        {
            _testContext.WaitForResult.HasErrored.Should().Be(false, $"handler should not have errored with error '{_testContext.WaitForResult.LastException?.Message}'");

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccount.Id, _testAccount.AccountLegalEntityId });

                account.Should().HaveCount(1);
                account.Should().BeEquivalentTo(_testAccount);
            }
        }
    }
}
