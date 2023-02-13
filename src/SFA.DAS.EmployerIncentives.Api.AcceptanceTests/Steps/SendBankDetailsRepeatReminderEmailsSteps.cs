using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendBankDetailsRepeatReminderEmails")]
    public class SendBankDetailsRepeatReminderEmailsSteps : StepsBase
    {
        private BankDetailsRepeatReminderEmailsRequest _request;
        private Account _account;
        private IncentiveApplication _application;
        private IncentiveApplicationApprenticeship _apprenticeship;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private DateTime _applicationCutOffDate;
        private HttpResponseMessage _response;

        public SendBankDetailsRepeatReminderEmailsSteps(TestContext testContext) : base(testContext) { }

        [When(@"an employer has submitted an application after the cut off date and not supplied bank details")]
        public async Task WhenAnEmployerHasSubmittedAnApplicationAndNotSuppliedBankDetails()
        {
            _applicationCutOffDate = DateTime.Today.AddDays(-10);

            _account = TestContext.TestData.GetOrCreate<Account>();
            _account.VrfCaseId = null;
            _account.VrfCaseStatusLastUpdatedDateTime = null;
            _account.VrfCaseStatus = null;
            _account.VrfVendorId = null;

            _application = TestContext.TestData.GetOrCreate<IncentiveApplication>();
            _application.AccountId = _account.Id;
            _application.AccountLegalEntityId = _account.AccountLegalEntityId;
            _application.Status = IncentiveApplicationStatus.Submitted;
            _application.DateCreated = _applicationCutOffDate.AddDays(-1);
            _application.DateSubmitted = _applicationCutOffDate.AddDays(-1);
            _apprenticeship = TestContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            _apprenticeship.IncentiveApplicationId = _application.Id;

            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeshipIncentive.SubmittedDate = _applicationCutOffDate.AddDays(-1);

            await SetupApprenticeshipIncentive();
        }

        private async Task SetupApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_application, true);
            await dbConnection.InsertAsync(_apprenticeship, true);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
            _apprenticeshipIncentive.PendingPayments = _apprenticeshipIncentive.PendingPayments.Take(2).ToList();
            _apprenticeshipIncentive.PendingPayments.First().EarningType = EarningType.FirstPayment;
            _apprenticeshipIncentive.PendingPayments.Last().EarningType = EarningType.SecondPayment;

            foreach (var pendingPayment in _apprenticeshipIncentive.PendingPayments)
            {
                pendingPayment.DueDate = pendingPayment.DueDate.Date;
                pendingPayment.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
                await dbConnection.InsertAsync(pendingPayment, true);
            }
        }

        [When(@"the check for accounts without bank details is triggered")]
        public async Task WhenTheCheckForAccountsWithoutBankDetailsIsTriggered()
        {
            const string url = "api/EmailCommand/bank-details-repeat-reminders";
            _request = new BankDetailsRepeatReminderEmailsRequest { ApplicationCutOffDate = _applicationCutOffDate };

            await TestContext.WaitFor(
               async (cancellationToken) =>
               {
                   _response = await EmployerIncentiveApi.Client.PostAsJsonAsync(url, _request, cancellationToken);
               },
               (context) => HasExpectedSendBankDetailsRepeatReminderEmailEvents(context)
               );

            _response.IsSuccessStatusCode.Should().BeTrue();
        }

        private bool HasExpectedSendBankDetailsRepeatReminderEmailEvents(TestContext testContext)
        {
            var processedEvents = testContext.CommandsPublished.Count(c =>
            c.IsProcessed &&
            c.Command is SendBankDetailsRepeatReminderEmailCommand);

            return processedEvents == 1;
        }

        [Then(@"the employer is sent a reminder email to supply their bank details in order to receive payment")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetailsInOrderToReceivePayment()
        {
            var directoryInfo = new DirectoryInfo($"{TestContext.MessageBus.StorageDirectory.FullName}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
            IOrderedEnumerable<FileInfo> recentFiles;
            try
            {
                recentFiles = directoryInfo.GetFiles().OrderByDescending(x => x.CreationTimeUtc >= DateTime.Now.AddMinutes(-2));
            }
            catch (DirectoryNotFoundException e)
            {
                Assert.Fail(e.Message + " Check query handlers to ensure domain commands were added");
                return;
            }

            foreach (var file in recentFiles)
            {
                var contents = File.ReadAllText(file.FullName, System.Text.Encoding.UTF8);

                if (contents.Contains(_apprenticeshipIncentive.SubmittedByEmail))
                {
                    Assert.Pass();
                    return;
                }
            }

            Assert.Fail($"No NServiceBus Message found with {_application.SubmittedByEmail}");
        }
    }
}
