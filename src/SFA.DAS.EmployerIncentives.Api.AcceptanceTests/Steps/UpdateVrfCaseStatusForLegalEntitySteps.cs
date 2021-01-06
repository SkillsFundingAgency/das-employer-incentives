using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
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
            DataAccess.SetupApplication(application);
            DataAccess.SetupApprenticeship(apprenticeship);
        }

        [When(@"VRF case status is changed to '(.*)'")]
        public async Task WhenVrfCaseStatusIsChangedTo(string status)
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

            var expectedEvents = 3;
            if(status == "Case Request Completed")
            {
                expectedEvents = 4;
            }

            await TestContext.WaitFor<ICommand>(async () =>
               await EmployerIncentiveApi.Patch(url, data), numberOfOnProcessedEventsExpected: expectedEvents);

            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"Employer Incentives account legal entity record is updated")]
        public void ThenEmployerIncentivesAccountLegalEntityRecordIsUpdated()
        {
            var publishedCommands = TestContext.DomainCommandsPublished.Where(c =>
                    c.IsPublished &&
                    c.Command is UpdateVendorRegistrationCaseStatusForAccountCommand                    
                ).ToList();

            foreach (var publishedCommand in publishedCommands)
            {
                var command = publishedCommand.Command as UpdateVendorRegistrationCaseStatusForAccountCommand;
                command.AccountId.Should().Be(_account.Id);
                command.HashedLegalEntityId.Should().Be(_account.HashedLegalEntityId);
                command.Status.Should().Be(_newVrfStatus);
                command.CaseId.Should().Be(_newVrfCaseId);
                command.LastUpdatedDate.Should().Be(_newVrfStatusUpdateDate);
                command.LockId.Should().Be($"{nameof(Account)}_{command.AccountId}");
            }

            var account = DataAccess.GetAccountByLegalEntityId(_account.LegalEntityId);
            account.Should().NotBeNull();
            account.VrfCaseId.Should().Be(_newVrfCaseId);
            account.VrfCaseStatus.Should().Be(_newVrfStatus);
            account.VrfCaseStatusLastUpdatedDateTime.Should().Be(_newVrfStatusUpdateDate);
        }

        [Then(@"a command to add an Employer Vendor Id Command is sent")]
        public void ThenACommandToAddAnEmployerVendorIdCommandIsSent()
        {
            var command = TestContext.CommandsPublished.Single(c =>
                    c.IsPublished &&
                    c.Command is AddEmployerVendorIdCommand).Command;

            ((AddEmployerVendorIdCommand)command).HashedLegalEntityId.Should().Be(_account.HashedLegalEntityId);
        }
    }
}
