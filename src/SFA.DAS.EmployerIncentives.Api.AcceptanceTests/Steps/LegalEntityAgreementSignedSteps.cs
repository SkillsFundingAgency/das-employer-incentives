using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LegalEntityAgreementSigned")]
    public class LegalEntityAgreementSignedSteps : StepsBase
    {
        private readonly Account _account;
        private HttpResponseMessage _response;
        private const int AgreementVersion = 5;

        public LegalEntityAgreementSignedSteps(TestContext testContext) : base(testContext)
        {
            _account = Fixture.Build<Account>()
                .Without(x => x.SignedAgreementVersion)
                .Without(x => x.VrfCaseId)
                .Without(x => x.VrfCaseStatus)
                .Without(x => x.VrfCaseStatusLastUpdatedDateTime)
                .Without(x => x.VrfVendorId)
                .Create();
        }

        [Given(@"the legal entity is already available in Employer Incentives")]
        public async Task GivenTheLegalEntityIsAlreadyAvailableInEmployerIncentives()
        {
            await DataAccess.SetupAccount(_account);
        }

        [Given(@"the legal entity is already available in Employer Incentives with a signed version")]
        public async Task GivenTheLegalEntityIsAlreadyAvailableInEmployerIncentivesWithASignedVersion()
        {
            _account.SignedAgreementVersion = AgreementVersion - 1;
            await DataAccess.SetupAccount(_account);
        }

        [When(@"the legal agreement is signed")]
        public async Task TheLegalAgreementIsSigned()
        {
            var url = $"/accounts/{_account.Id}/legalEntities/{_account.AccountLegalEntityId}";
            var request = Fixture.Build<SignAgreementRequest>()
                .With(r => r.AgreementVersion, AgreementVersion).Create();
            _response = await EmployerIncentiveApi.Patch(url, request, TestContext.CancellationToken);
        }

        [Then(@"the employer can apply for incentives")]
        public async Task TheEmployerCanApplyForIncentives()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var accounts = await dbConnection.GetAllAsync<Account>();
            var account = accounts.Single(a =>
                a.Id == _account.Id && a.AccountLegalEntityId == _account.AccountLegalEntityId);

            account.SignedAgreementVersion.Should().Be(AgreementVersion);
        }

    }
}
