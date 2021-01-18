using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Extensions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
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
        private TestContext _testContext;
        private GetApplicationsResponse _apiResponse;
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;

        public ApplicationsForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [Given(@"an account that is in employer incentives")]
        public async Task GivenAnAccountThatIsInEmployerIncentives()
        {
            _account = _testContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = _testContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await SetupAccount(_account);
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

            apprenticeshipApplication.AccountId.Should().Be(_account.Id);
            //apprenticeshipApplication.ApplicationId.Should().Be(_application.Id); //TODO: This needs resolving as the bank details needs the original application id
            apprenticeshipApplication.FirstName.Should().Be(_apprenticeshipIncentive.FirstName);
            apprenticeshipApplication.LastName.Should().Be(_apprenticeshipIncentive.LastName);
            apprenticeshipApplication.LegalEntityName.Should().Be(_account.LegalEntityName);
            apprenticeshipApplication.TotalIncentiveAmount.Should().Be(_apprenticeshipIncentive.PendingPayments.Sum(x => x.Amount));
            apprenticeshipApplication.SubmittedByEmail.Should().Be(_apprenticeshipIncentive.SubmittedByEmail);
            apprenticeshipApplication.ApplicationDate.Date.Should().Be(_apprenticeshipIncentive.SubmittedDate.Value.Date);
        }

        private async Task SetupAccount(Account account)
        {
            using (var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync($"insert into Accounts(id, accountLegalEntityId, legalEntityId, legalEntityName, hasSignedIncentivesTerms) values " +
                                                $"(@id, @accountLegalEntityId, @legalEntityId, @legalEntityName, @hasSignedIncentivesTerms)", account);
            }
        }

        private async Task SetupApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            foreach (var pendingPayment in _apprenticeshipIncentive.PendingPayments)
            {
                pendingPayment.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
                await dbConnection.InsertWithEnumAsString(pendingPayment);
            }
        }
    }
}
