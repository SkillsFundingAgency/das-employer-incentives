using System.Linq;
using FluentAssertions;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityAgreementSigned")]
    public class LegalEntityAgreementSignedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;

        public LegalEntityAgreementSignedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            var fixture = new Fixture();
            _testAccountTable = _testContext.TestData.GetOrCreate<Account>(null, () => fixture.Build<Account>().With(x => x.HasSignedIncentivesTerms, false).Create());
        }

        [When(@"the legal agreement is signed")]
        public async Task TheLegalAgreementIsSigned()
        {
            var url = $"/accounts/{_testAccountTable.Id}/legalEntities/{_testAccountTable.AccountLegalEntityId}";
            await EmployerIncentiveApi.Patch(url, new SignAgreementRequest { AgreementVersion = 5 });
        }

        [Then(@"the employer can apply for incentives")]
        public async Task TheEmployerCanApplyForIncentives()
        {
            EmployerIncentiveApi.GetLastResponse().StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccountTable.Id, _testAccountTable.AccountLegalEntityId });

                account.Should().HaveCount(1);
                account.Single().HasSignedIncentivesTerms.Should().BeTrue();
            }
        }
    }
}
