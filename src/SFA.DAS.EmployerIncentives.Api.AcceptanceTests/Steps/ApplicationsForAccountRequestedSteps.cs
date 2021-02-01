using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ApplicationsForAccountRequested")]
    public class ApplicationsForAccountRequestedSteps : StepsBase
    {
        private GetApplicationsResponse _apiResponse;
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;

        public ApplicationsForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an account that is in employer incentives")]
        public async Task GivenAnAccountThatIsInEmployerIncentives()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await SetupApprenticeshipIncentive();
        }

        [When(@"a client requests the apprenticeships for the account")]
        public async Task WhenAClientRequestsTheApprenticeshipApplicationsForTheAccount()
        {
            var url = $"/accounts/{_account.Id}/legalentity/{_account.AccountLegalEntityId}/applications";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetApplicationsResponse>(url);

            status.Should().Be(HttpStatusCode.OK);

            _apiResponse = data;
        }

        [Then(@"the apprenticeships are returned")]
        public void ThenTheApprenticeshipApplicationsAreReturned()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var currentPeriodDate = new DateTime(TestContext.ActivePeriod.CalendarYear, TestContext.ActivePeriod.CalendarMonth, 1);

            apprenticeshipApplication.AccountId.Should().Be(_account.Id);
            apprenticeshipApplication.FirstName.Should().Be(_apprenticeshipIncentive.FirstName);
            apprenticeshipApplication.LastName.Should().Be(_apprenticeshipIncentive.LastName);
            apprenticeshipApplication.LegalEntityName.Should().Be(_account.LegalEntityName);
            apprenticeshipApplication.TotalIncentiveAmount.Should().Be(_apprenticeshipIncentive.PendingPayments.Sum(x => x.Amount));
            apprenticeshipApplication.SubmittedByEmail.Should().Be(_apprenticeshipIncentive.SubmittedByEmail);
            // ReSharper disable once PossibleInvalidOperationException
            apprenticeshipApplication.ApplicationDate.Date.Should().Be(_apprenticeshipIncentive.SubmittedDate.Value.Date);

            apprenticeshipApplication.FirstPaymentStatus.Should().BeEquivalentTo(new
            {
                PaymentAmount = _apprenticeshipIncentive.PendingPayments.First().Amount,
                PaymentDate = currentPeriodDate <= _apprenticeshipIncentive.PendingPayments.First().DueDate ? new DateTime(currentPeriodDate.AddMonths(1).Year, currentPeriodDate.AddMonths(1).Month, _apprenticeshipIncentive.PendingPayments.First().DueDate.Day) : _apprenticeshipIncentive.PendingPayments.First().DueDate.AddMonths(1)
            });
            apprenticeshipApplication.SecondPaymentStatus.Should().BeEquivalentTo(new
            {
                PaymentAmount = _apprenticeshipIncentive.PendingPayments.Last().Amount,
                PaymentDate = _apprenticeshipIncentive.PendingPayments.Last().DueDate.AddMonths(1)
            });
        }

        private async Task SetupApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
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
    }
}
