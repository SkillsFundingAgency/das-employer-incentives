using System;
using AutoFixture;
using Dapper;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    public class LegalEntitySteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;

        public LegalEntitySteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            var fixture = new Fixture();
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>(null,
                () => fixture.Build<Account>()
                    .With(x => x.HasSignedIncentivesTerms, false)
                    .With(x => x.VrfCaseId, (string)null)
                    .With(x => x.VrfCaseStatus, (string)null)
                    .With(x => x.VrfCaseStatusLastUpdatedDateTime, (DateTime?)null)
                    .With(x => x.VrfVendorId, (string)null).Create());
        }

        [Given(@"a legal entity is not available in employer incentives")]
        [Given(@"the legal entity is in not Employer Incentives")]
        public async Task GivenIHaveALegalEntityThatIsNotInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccountTable.Id, _testAccountTable.AccountLegalEntityId });

                account.Should().BeNullOrEmpty();
            }
        }

        [Given(@"a legal entity that is in employer incentives")]
        [Given(@"the legal entity is already available in Employer Incentives")]
        public void GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            DataAccess.SetupAccount(_testAccountTable);
        }

        [Then(@"the legal entity should no longer be available in employer incentives")]
        [Then(@"the legal entity is still not available in employer incentives")]
        [Then(@"the legal entity should not be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldNotBeAvailableInTheDatabase()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccountTable.Id, _testAccountTable.AccountLegalEntityId });

                account.Should().HaveCount(0);
            }
        }

        [Then(@"the legal entity should be available in Employer Incentives")]
        [Then(@"the legal entity should still be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldBeAvailableInTheDatabase()
        {
            //var account = DataAccess.GetAccountByLegalEntityId()
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccountTable.Id, _testAccountTable.AccountLegalEntityId });

                account.Should().HaveCount(1);
                account.Single().Should().BeEquivalentTo(_testAccountTable, opts => opts.Excluding(x => x.HashedLegalEntityId));
                account.Single().HashedLegalEntityId.Should().NotBeNullOrEmpty();
            }
        }
    }
}
