using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
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
        private string _newVrfStatus;
        private readonly DateTime _newVrfStatusUpdateDate = DateTime.Parse("01-01-2020");
        private Account _account;

        public UpdateVrfCaseStatusForLegalEntitySteps(TestContext testContext) : base(testContext)
        {

        }

        [Given(@"an existing submitted incentive application")]
        public async Task GivenAnExistingSubmittedIncentiveApplication()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            var application = TestContext.TestData.GetOrCreate<IncentiveApplication>();
            application.AccountId = _account.Id;
            application.AccountLegalEntityId = _account.AccountLegalEntityId;
            var apprenticeship = TestContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            apprenticeship.IncentiveApplicationId = application.Id;

            await DataAccess.SetupAccount(_account);
            await DataAccess.InsertApplication(application);
            await DataAccess.Insert(apprenticeship);
        }

        [When(@"VRF case status is changed to '(.*)'")]
        public async Task WhenVRFCaseStatusIsChangedTo(string status)
        {
            var url = $"/legalentities/{_account.HashedLegalEntityId}/vendorregistrationform/status";

            _newVrfStatus = status;

            var data = new
            {
                CaseId = _newVrfCaseId,
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
            account.VrfCaseStatus.Should().Be(_newVrfStatus);
            account.VrfCaseStatusLastUpdatedDateTime.Should().Be(_newVrfStatusUpdateDate);
        }

        [Then(@"a command to add an Employer Vendor Id Command is sent")]
        public void ThenACommandToAddAnEmployerVendorIdCommandIsSent()
        {
            var command = TestContext.CommandsPublished.Single(c => c.IsPublished).Command;
            command.Should().BeOfType<AddEmployerVendorIdCommand>();
            ((AddEmployerVendorIdCommand)command).HashedLegalEntityId.Should().Be(_account.HashedLegalEntityId);
        }
    }
}
