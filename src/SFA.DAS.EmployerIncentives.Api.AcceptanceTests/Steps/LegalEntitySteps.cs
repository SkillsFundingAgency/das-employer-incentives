using AutoFixture;
using Dapper.Contrib.Extensions;
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
        private readonly Account _account;

        public LegalEntitySteps(TestContext testContext) : base(testContext)
        {
            var fixture = new Fixture();
            _account = TestContext.TestData.GetOrCreate(null,
                () => fixture.Build<Account>()
                    .Without(x => x.SignedAgreementVersion)
                    .Without(x => x.VrfCaseId)
                    .Without(x => x.VrfCaseStatus)
                    .Without(x => x.VrfCaseStatusLastUpdatedDateTime)
                    .Without(x => x.VendorBlockEndDate)
                    .Without(x => x.VrfVendorId).Create());
        }

        [Given(@"the legal entity is in not available in Employer Incentives")]
        public void GivenTheLegalEntityIsInNotAvailableInEmployerIncentives()
        {
            // intentionally blank
        }

        [Given(@"a legal entity that is in employer incentives")]
        public Task GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            return DataAccess.SetupAccount(_account);
        }

        [Then(@"the legal entity should no longer be available in employer incentives")]
        [Then(@"the legal entity is still not available in employer incentives")]
        [Then(@"the legal entity should not be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldNotBeAvailableInTheDatabase()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var accounts = await dbConnection.GetAllAsync<Account>();
            accounts.Any(a => a.Id == _account.Id && a.AccountLegalEntityId == _account.AccountLegalEntityId)
                .Should().BeFalse();
        }

        [Then(@"the legal entity should be available in Employer Incentives")]
        [Then(@"the legal entity should still be available in Employer Incentives")]
        public async Task ThenTheLegalEntityShouldBeAvailableInTheDatabase()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var accounts = await dbConnection.GetAllAsync<Account>();
            var account = accounts.Single(a => a.Id == _account.Id && a.AccountLegalEntityId == _account.AccountLegalEntityId);

            account.Should().BeEquivalentTo(_account,
            opts => opts.Excluding(x => x.HashedLegalEntityId)
                    .Excluding(x => x.VrfCaseStatusLastUpdatedDateTime)
                    .Excluding(x => x.HasBeenDeleted)
                    .Excluding(x => x.VendorBlockEndDate));
            account.HashedLegalEntityId.Should().NotBeNullOrEmpty();
        }
    }
}
