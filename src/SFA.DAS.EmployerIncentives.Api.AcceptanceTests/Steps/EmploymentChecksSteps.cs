using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentChecks")]
    public class EmploymentChecksSteps : StepsBase
    {
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private EmploymentCheck _employmentCheck;
        private Guid _correlationId;
        private HttpResponseMessage _response;
        private UpdateEmploymentCheckCommand _delayedCommand;

        public EmploymentChecksSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an apprenticeship incentive has submitted a new employment check")]
        public async Task GivenAnApprenticeshipIncentiveHasSubmittedANewEmploymentCheck()
        {
            _correlationId = Guid.NewGuid();
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeshipIncentive.EmploymentChecks.Clear();

            _employmentCheck = Fixture.Build<EmploymentCheck>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.CorrelationId, _correlationId)
                .With(p => p.CreatedDateTime, DateTime.Today.AddDays(-10))
                .Without(p => p.Result)
                .Without(p => p.ResultDateTime)
                .Without(p => p.UpdatedDateTime)
                .Create();
            
            _apprenticeshipIncentive.EmploymentChecks.Add(_employmentCheck);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
            await dbConnection.InsertAsync(_employmentCheck, false);
        }

        [Given(@"the employment check result processing has been delayed")]
        public void GivenTheEmploymentCheckResultProcessingHasBeenDelayed()
        {
            _delayedCommand = new UpdateEmploymentCheckCommand(_correlationId, EmploymentCheckResultType.Employed, DateTime.Today);              
        }

        [When(@"the employment check result is returned during month end payment process is running")]
        public async Task WhenEmploymentCheckResultIsReturnedDuringMonthEndPaymentProcessIsRunning()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<CollectionCalendarPeriod>();
            var period = calendar.Single(x => x.Active);
            period.PeriodEndInProgress = true;
        
            await dbConnection.UpdateAsync(period);

            await WhenTheEmploymentCheckResultIsReturnedWithTheResult(EmploymentCheckResultType.Employed);
        }

        [When(@"the employment check result processing resumes")]
        public async Task WhenEmploymentCheckResultProcessingResumes()
        {
            await TestContext.WaitFor<ICommand>(async (cancellationToken) =>
                await TestContext.MessageBus.Send(_delayedCommand), numberOfOnProcessedEventsExpected: 1);
        }

        [When(@"the employment check result is returned with a result of (.*)")]
        public async Task WhenTheEmploymentCheckResultIsReturnedWithTheResult(EmploymentCheckResultType checkResultType)
        { 
            await TestContext.WaitFor(
                async (cancellationToken) =>
                {
                    _response = await EmployerIncentiveApi.Post(
                    $"/employmentCheckResults",
                    new UpdateEmploymentCheckRequest
                    {
                        CorrelationId = _correlationId,
                        Result = checkResultType.ToString(),
                        DateChecked = DateTime.Today
                    });
                },
                (context) => HasProcessedCommand(context),
                assertOnError: false
                );

            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private bool HasProcessedCommand(TestContext testContext)
        {
            return testContext.CommandsPublished.Count(c => c.IsProcessed &&
                    c.IsDomainCommand &&
                    c.Command is UpdateEmploymentCheckCommand) == 1;
        }

        [Then(@"the apprenticeship incentive employment check result is updated to (.*)")]
        public async Task ThenTheAprenticeshipIncentiveEmploymentCheckIsUpdatedTo(bool hasPassed)
        {
            TestContext.CommandsPublished.Count(c => c.IsDelayed &&
                   c.IsDomainCommand &&
                   c.Command is UpdateEmploymentCheckCommand).Should().Be(0);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentChecks = await dbConnection.GetAllAsync<EmploymentCheck>();

            employmentChecks.Count().Should().Be(1);
            var employmentCheck = employmentChecks.Single();
            
            employmentCheck.CorrelationId.Should().Be(_correlationId);
            employmentCheck.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            employmentCheck.Result.Should().Be(hasPassed);
            employmentCheck.ResultDateTime.Should().Be(DateTime.Today);
            employmentCheck.UpdatedDateTime.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));
        }

        [Then(@"the apprenticeship incentive employment check result processing is delayed")]
        public async Task ThenTheAprenticeshipIncentiveEmploymentCheckIsDelayed()
        {
            TestContext.CommandsPublished.Count(c => c.IsDelayed &&
                   c.IsDomainCommand &&
                   c.Command is UpdateEmploymentCheckCommand).Should().Be(1);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentChecks = await dbConnection.GetAllAsync<EmploymentCheck>();

            employmentChecks.Count().Should().Be(1);
            var employmentCheck = employmentChecks.Single();

            employmentCheck.CorrelationId.Should().Be(_correlationId);
            employmentCheck.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            employmentCheck.Result.Should().BeNull();
            employmentCheck.ResultDateTime.Should().BeNull();
            employmentCheck.UpdatedDateTime.Should().BeNull();
        }

        [Then(@"the apprenticeship incentive employment check result is processed")]
        public async Task ThenTheAprenticeshipIncentiveEmploymentCheckIsProcessed()
        {
            await ThenTheAprenticeshipIncentiveEmploymentCheckIsUpdatedTo(true);
        }
    }
}
