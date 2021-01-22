using Dapper;
using FluentAssertions;
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
        private IncentiveApplication _application;
        private IncentiveApplicationApprenticeship _apprenticeship;

        public ApplicationsForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
        }

        [Given(@"an account that is in employer incentives")]
        public async Task GivenAnAccountThatIsInEmployerIncentives()
        {
            _account = _testContext.TestData.GetOrCreate<Account>();
            _application = _testContext.TestData.GetOrCreate<IncentiveApplication>();
            _application.AccountId = _account.Id;
            _application.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeship = _testContext.TestData.GetOrCreate<IncentiveApplicationApprenticeship>();
            _apprenticeship.IncentiveApplicationId = _application.Id;

            await SetupAccount(_account);
            await SetupApplication(_application);
            await SetupApprenticeship(_apprenticeship);
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
            apprenticeshipApplication.ApplicationId.Should().Be(_application.Id);
            apprenticeshipApplication.FirstName.Should().Be(_apprenticeship.FirstName);
            apprenticeshipApplication.LastName.Should().Be(_apprenticeship.LastName);
            apprenticeshipApplication.LegalEntityName.Should().Be(_account.LegalEntityName);
            apprenticeshipApplication.Status.Should().Be(_application.Status.ToString());
            apprenticeshipApplication.TotalIncentiveAmount.Should().Be(_apprenticeship.TotalIncentiveAmount);
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
