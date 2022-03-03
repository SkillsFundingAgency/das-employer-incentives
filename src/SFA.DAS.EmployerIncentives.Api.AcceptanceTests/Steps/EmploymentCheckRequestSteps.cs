using SFA.DAS.EmployerIncentives.Data.Models;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using System.Data.SqlClient;
using System.Linq;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using WireMock.RequestBuilders;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentCheckRequest")]
    public class EmploymentCheckRequestSteps : StepsBase
    {
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private ServiceRequest _serviceRequest;
        private HttpResponseMessage _response;
        private RefreshEmploymentCheckCommand _delayedCommand;

        public EmploymentCheckRequestSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an apprenticeship incentive has been submitted")]
        public async Task GivenAnApprenticeshipIncentiveHasBeenSubmitted()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _serviceRequest = TestContext.TestData.GetOrCreate<ServiceRequest>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.StartDate = DateTime.Now.AddDays(-42);
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.Phase = Enums.Phase.Phase2;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
        }

        [Given(@"an apprenticeship incentive has not been submitted")]
        public async Task GivenAnApprenticeshipIncentiveHasNotBeenSubmitted()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _serviceRequest = TestContext.TestData.GetOrCreate<ServiceRequest>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            // no data stored
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            // no incentive record
        }

        [Given(@"the active period month end processing is in progress")]
        public async Task GivenTheActivePeriodMonthEndProcessingIsInProgress()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            TestContext.ActivePeriod.PeriodEndInProgress = true;
            await dbConnection.UpdateAsync(TestContext.ActivePeriod);
        }

        [Given(@"an employment check refresh has been requested by support")]
        public async Task GivenAnEmploymentCheckRefreshHasBeenRequestedBySupport()
        {
            await GivenAnApprenticeshipIncentiveHasBeenSubmitted();
        }

        [Given(@"the employment check refresh processing has been delayed")]
        public void GivenTheEmploymentCheckRefreshProcessingHasBeenDelayed()
        {
            _delayedCommand = new RefreshEmploymentCheckCommand(
                _apprenticeshipIncentive.AccountLegalEntityId,
                _apprenticeshipIncentive.ULN,
                _serviceRequest.TaskId,
                _serviceRequest.DecisionReference,
                _serviceRequest.TaskCreatedDate);
        }

        [Given(@"the employment checks feature toggle is set to (.*)")]
        public void GivenTheEmploymentChecksFeatureToggleIsSet(bool featureToggle)
        {
            TestContext.ApplicationSettings.EmploymentCheckEnabled = featureToggle;
        }

        [When(@"the employment check refresh processing resumes")]
        public async Task WhenEmploymentCheckRefreshProcessingResumes()
        {
            await TestContext.WaitFor<ICommand>(async (cancellationToken) =>
                await TestContext.MessageBus.Send(_delayedCommand), numberOfOnProcessedEventsExpected: 1);
        }

        [When(@"an employment check refresh is requested")]
        public async Task WhenAnEmploymentCheckRefreshIsRequested()
        {
            var data = new Dictionary<string, object>
            {
                { "AccountLegalEntityId", _apprenticeshipIncentive.AccountLegalEntityId },
                { "ULN", _apprenticeshipIncentive.ULN },
                { "ServiceRequest", JsonConvert.SerializeObject(_serviceRequest) }
            };
            
            await TestContext.WaitFor(
               async (cancellationToken) =>
               {
                   _response = await EmployerIncentiveApi.Put(
                    "/jobs",
                   new JobRequest { Type = JobType.RefreshEmploymentCheck, Data = data },
                   TestContext.CancellationToken);
               },
               (context) => HasProcessedCommand(context),
               assertOnError: false
               );

            _response.EnsureSuccessStatusCode();
        }

        private bool HasProcessedCommand(TestContext testContext)
        {
            return testContext.CommandsPublished.Count(c => 
                    c.IsProcessed &&
                    c.IsDomainCommand &&
                    c.Command is RefreshEmploymentCheckCommand) == 1;
        }

        [Then(@"a request is made to refresh the employment checks for the incentive")]
        public async Task ThenARequestIsMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Should().Be(1);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var audits = await dbConnection.GetAllAsync<EmploymentCheckAudit>();
            audits.Should().HaveCount(1);
            var audit = audits.Single();
            audit.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audit.ServiceRequestTaskId.Should().Be(_serviceRequest.TaskId);
            audit.ServiceRequestDecisionReference.Should().Be(_serviceRequest.DecisionReference);
            audit.ServiceRequestCreatedDate.Should().Be(_serviceRequest.TaskCreatedDate.Value);
        }

        [Then(@"a request is not made to refresh the employment checks for the incentive")]
        public void ThenARequestIsNotMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Should().Be(0);
        }

        [Then(@"a request is not made to the employment checks API")]
        public void ThenARequestIsNotMadeToTheEmploymentChecksAPI()
        {
            var requests = TestContext
                .EmploymentCheckApi
                .MockServer
                .FindLogEntries(
                    Request
                        .Create()
                        .WithPath(u => u.StartsWith($"/employmentchecks"))
                        .UsingPut());

            requests.AsEnumerable().Count().Should().Be(0);
        }

        [Then(@"the request to refresh the employment checks for the incentive is delayed")]
        public void ThenTheRequestToRefreshTheEmploymentChecksForTheIncentiveIsDelayed()
        {
            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsDelayed &&
                    c.Command is RefreshEmploymentCheckCommand)
                .Should().Be(1);

            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Should().Be(0);
        }
    }
}
