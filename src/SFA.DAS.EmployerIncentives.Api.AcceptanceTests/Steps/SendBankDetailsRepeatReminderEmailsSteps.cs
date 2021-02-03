using AutoFixture;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
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
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private BankDetailsRepeatReminderEmailsRequest _request;
        private readonly string _url;
        private readonly string _storageDirectory;
        private Account _account;
        private IncentiveApplication _application;
        private IncentiveApplicationApprenticeship _apprenticeship;
        private DateTime _applicationCutOffDate;
        private HttpResponseMessage _response;

        public SendBankDetailsRepeatReminderEmailsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _url = "/api/EmailCommand/bank-details-required";
            _storageDirectory = testContext.MessageBus.StorageDirectory.FullName;
        }

        [When(@"an employer has submitted an application after the cut off date and not supplied bank details")]
        public async Task WhenAnEmployerHasSubmittedAnApplicationAndNotSuppliedBankDetails()
        {
            _applicationCutOffDate = DateTime.Today.AddDays(-10);

            _account = _testContext.TestData.GetOrCreate<Account>();
            _application = _testContext.TestData.GetOrCreate<IncentiveApplication>();
            _application.AccountId = _account.Id;
            _application.AccountLegalEntityId = _account.AccountLegalEntityId;
            _application.Status = Enums.IncentiveApplicationStatus.Submitted;
            _application.DateCreated = _applicationCutOffDate.AddDays(-1);
            _application.DateSubmitted = _applicationCutOffDate.AddDays(-1);
            _apprenticeship = _testContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            _apprenticeship.IncentiveApplicationId = _application.Id;

            await SetupAccount(_account);
            await SetupApplication(_application);
            await SetupApprenticeship(_apprenticeship);
        }

        [When(@"the check for accounts without bank details is triggered")]
        public async Task WhenTheCheckForAccountsWithoutBankDetailsIsTriggered()
        {
            var url = "api/EmailCommand/bank-details-repeat-reminders";
            _request = new BankDetailsRepeatReminderEmailsRequest { ApplicationCutOffDate = _applicationCutOffDate };

            await _testContext.WaitFor<ICommand>(async (cancellationToken) =>
            {
                _response = await EmployerIncentiveApi.Client.PostAsJsonAsync(url, _request, cancellationToken);
            },
            numberOfOnProcessedEventsExpected: 2);

            _response.IsSuccessStatusCode.Should().BeTrue();
        }

        [Then(@"the employer is sent a reminder email to supply their bank details in order to receive payment")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetailsInOrderToReceivePayment()
        {
            var directoryInfo = new DirectoryInfo($"{_storageDirectory}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
            var recentFiles = directoryInfo.GetFiles().OrderByDescending(x => x.CreationTimeUtc >= DateTime.Now.AddMinutes(-2));

            foreach (var file in recentFiles)
            {
                var contents = File.ReadAllText(file.FullName, Encoding.UTF8);

                if (contents.Contains(_application.SubmittedByEmail))
                {
                    return;
                }
            }

            Assert.Fail($"No NServiceBus Message found with {_application.SubmittedByEmail}");
        }

        private async Task SetupAccount(Account account)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into Accounts(id, accountLegalEntityId, legalEntityId, legalEntityName, hasSignedIncentivesTerms) values " +
                                                $"(@id, @accountLegalEntityId, @legalEntityId, @legalEntityName, @hasSignedIncentivesTerms)", account);
            }
        }

        private async Task SetupApplication(IncentiveApplication application)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into IncentiveApplication(id, accountId, accountLegalEntityId, dateCreated, status, dateSubmitted, submittedByEmail, submittedByName) values " +
                                                $"(@id, @accountId, @accountLegalEntityId, @dateCreated, @status, @dateSubmitted, @submittedByEmail, @submittedByName)", application);
            }
        }

        private async Task SetupApprenticeship(IncentiveApplicationApprenticeship apprenticeship)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into IncentiveApplicationApprenticeship(id, incentiveApplicationId, apprenticeshipId, firstName, lastName, dateOfBirth, " +
                                                "uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval, TotalIncentiveAmount) values " +
                                                "(@id, @incentiveApplicationId, @apprenticeshipId, @firstName, @lastName, @dateOfBirth, " +
                                                "@uln, @plannedStartDate, @apprenticeshipEmployerTypeOnApproval, @totalIncentiveAmount)", apprenticeship);
            }
        }
    }
}
