using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    public class UpdateVrfCaseDetailsForLegalEntitySteps : StepsBase
    {
        private readonly DataAccess _dataAccess;
        private readonly string _newVrfCaseId = $"NewVrfCaseId{Guid.NewGuid()}";
        private readonly string _newVrfVendorId = $"NewVrfVendorId{Guid.NewGuid()}";
        private readonly string _newVrfStatus = $"NewVrfStatus{Guid.NewGuid()}";
        private Account _account;

        public UpdateVrfCaseDetailsForLegalEntitySteps(TestContext testContext) : base(testContext)
        {
            _dataAccess = new DataAccess(testContext.SqlDatabase.DatabaseInfo.ConnectionString);
        }

        [Given(@"an existing submitted application")]
        public void AnApplicationHasBeenSubmitted()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            var application = TestContext.TestData.GetOrCreate<IncentiveApplication>();
            application.AccountId = _account.Id;
            application.AccountLegalEntityId = _account.AccountLegalEntityId;
            var apprenticeship = TestContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            apprenticeship.IncentiveApplicationId = application.Id;

            _dataAccess.SetupAccount(_account);
            _dataAccess.SetupApplication(application);
            _dataAccess.SetupApprenticeship(apprenticeship);
        }

        [When(@"VRF case, vendor and status are changed")]
        public async Task VrfCaseVendorAndStatusAreChanged()
        {
            var url = $"/legalentities/{_account.LegalEntityId}/vendorregistrationform";
            var data = new
            {
                CaseId = _newVrfCaseId,
                VendorId = _newVrfVendorId,
                Status = _newVrfStatus,
                LegalEntityId = _account.LegalEntityId
            };

            await EmployerIncentiveApi.Patch(url, data);
        }

        [Then(@"Employer Incentives legal entity record is updated")]
        public void ThenTheEmployerIncentivesLegalEntityRecordIsUpdated()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var account = _dataAccess.GetAccountByLegalEntityId(_account.LegalEntityId);
            account.Should().NotBeNull();
            account.VrfCaseId.Should().Be(_newVrfCaseId);
            account.VrfVendorId.Should().Be(_newVrfVendorId);
            account.VrfCaseStatus.Should().Be(_newVrfStatus);
        }

    }
}
