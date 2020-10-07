using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "UpdateVrfCaseStatusForLegalEntity")]
    public class UpdateVrfCaseStatusForLegalEntitySteps : StepsBase
    {
        private readonly string _newVrfCaseId = $"NewVrfCaseId{Guid.NewGuid()}";
        private readonly string _newVrfVendorId = $"NewVrfVendorId{Guid.NewGuid()}";
        private readonly string _newVrfStatus = $"NewVrfStatus{Guid.NewGuid()}";
        private readonly DateTime _newVrfStatusUpdateDate = DateTime.Parse("01-01-2020");
        private Account _account;

        public UpdateVrfCaseStatusForLegalEntitySteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an existing submitted incentive application")]
        public void GivenAnExistingSubmittedIncentiveApplication()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            var application = TestContext.TestData.GetOrCreate<IncentiveApplication>();
            application.AccountId = _account.Id;
            application.AccountLegalEntityId = _account.AccountLegalEntityId;
            var apprenticeship = TestContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            apprenticeship.IncentiveApplicationId = application.Id;

            DataAccess.SetupAccount(_account);
            DataAccess.SetupApplication(application);
            DataAccess.SetupApprenticeship(apprenticeship);
        }

        [When(@"VRF case status is changed")]
        public async Task WhenVRFCaseStatusIsChanged()
        {
            var url = $"/legalentities/{_account.HashedLegalEntityId}/vendorregistrationform/status";
            var data = new
            {
                CaseId = _newVrfCaseId,
                VendorId = _newVrfVendorId,
                Status = _newVrfStatus,
                LegalEntityId = _account.HashedLegalEntityId,
                CaseStatusLastUpdatedDate = _newVrfStatusUpdateDate
            };

            await EmployerIncentiveApi.Patch(url, data);
        }

        [Then(@"Employer Incentives account legal entity record is updated")]
        public void ThenEmployerIncentivesAccountLegalEntityRecordIsUpdated()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var account = DataAccess.GetAccountByLegalEntityId(_account.LegalEntityId);
            account.Should().NotBeNull();
            account.VrfCaseId.Should().Be(_newVrfCaseId);
            account.VrfVendorId.Should().Be(_newVrfVendorId);
            account.VrfCaseStatus.Should().Be(_newVrfStatus);
            account.VrfCaseStatusLastUpdatedDateTime.Should().Be(_newVrfStatusUpdateDate);
        }
    }
}
