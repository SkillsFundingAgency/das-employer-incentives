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
using System.Net.Http;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityAgreementSigned")]
    public class LegalEntityAgreementSignedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Account _testAccountTable;
        private HttpResponseMessage _response;
        private readonly int _agreementVersion = 5;

        public LegalEntityAgreementSignedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            var fixture = new Fixture();
            _testAccountTable = _testContext.TestData.GetOrCreate(null, () => 
                fixture
                .Build<Account>()
                .With(x => x.HasSignedIncentivesTerms, false)
                .With(x => x.SignedAgreementVersion, (int?)null)
                .Create());            
        }

        [Given(@"the legal entity is already available in Employer Incentives with a signed version")]
        public Task GivenIHaveALegalEntityThatIsAlreadyInTheDatabase()
        {
            _testAccountTable.HasSignedIncentivesTerms = true;
            _testAccountTable.SignedAgreementVersion = _agreementVersion - 1;
            return DataAccess.SetupAccount(_testAccountTable);
        }

        [When(@"the legal agreement is signed")]
        public async Task TheLegalAgreementIsSigned()
        {
            var url = $"/accounts/{_testAccountTable.Id}/legalEntities/{_testAccountTable.AccountLegalEntityId}";
            _response = await EmployerIncentiveApi.Patch(url, new SignAgreementRequest { AgreementVersion = _agreementVersion });
        }

        [Then(@"the employer can apply for incentives")]
        public async Task TheEmployerCanApplyForIncentives()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                var account = await dbConnection.QueryAsync<Account>("SELECT * FROM Accounts WHERE Id = @Id AND AccountLegalEntityId = @AccountLegalEntityId",
                    new { _testAccountTable.Id, _testAccountTable.AccountLegalEntityId });

                account.Should().HaveCount(1);
                account.Single().HasSignedIncentivesTerms.Should().BeTrue();
                account.Single().SignedAgreementVersion.Should().Be(_agreementVersion);
            }
        }
    }
}
