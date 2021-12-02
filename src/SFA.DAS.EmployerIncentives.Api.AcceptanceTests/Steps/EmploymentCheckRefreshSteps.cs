using SFA.DAS.EmployerIncentives.Data.Models;
using System.Threading.Tasks;
using AutoFixture;
using TechTalk.SpecFlow;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentCheckRefresh")]
    public class EmploymentCheckRefreshSteps : StepsBase
    {
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private Fixture _fixture;

        public EmploymentCheckRefreshSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
        }

        [Given(@"an apprenticeship incentive has been submitted")]
        public async Task GivenAnApprenticeshipIncentiveHasSubmittedANewEmploymentCheck()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeshipIncentive.EmploymentChecks.Clear();

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
        }

        [Given(@"a learner match has been performed for the incentive with a learning found result of (.*)")]
        public async Task GivenALearnerMatchHasBeenPerformedForTheIncentive(bool learningFound)
        {
            var learner = _fixture
                .Build<Learner>()
                .With(p => p.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.ULN, _apprenticeshipIncentive.ULN)
                .With(p => p.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(p => p.LearningFound, learningFound)
                .Create();

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(learner, false);
        }

        [Given(@"a learner match has not been performed")]
        public void GivenALearnerMatchHasNotBeenPerformed()
        {

        }

        [When(@"the employment checks are refreshed")]
        public async Task WhenTheEmploymentChecksAreRefreshed()
        {
            var url = $"employmentchecks/refresh";
            var response = await EmployerIncentiveApi.Put(url, string.Empty);
            response.EnsureSuccessStatusCode();
        }

        [Then(@"a request is made to refresh the employment checks for the incentive")]
        public void ThenARequestIsMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Where(c =>
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Select(c => c.Command)
                .Count().Should().Be(1);
        }


        [Then(@"a request is not made to refresh the employment checks for the incentive")]
        public void ThenARequestIsNotMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Where(c =>
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Select(c => c.Command)
                .Count().Should().Be(0);
        }
    }
}
