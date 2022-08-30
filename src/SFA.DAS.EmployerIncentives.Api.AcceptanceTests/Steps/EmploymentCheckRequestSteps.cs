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
using System.Net;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Enums;
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
            _apprenticeshipIncentive.StartDate = DateTime.Now.AddYears(-1).AddDays(-42);
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.Phase = Phase.Phase2;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;
            _apprenticeshipIncentive.Status = IncentiveStatus.Active;

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, true);
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
                RefreshEmploymentCheckType.InitialEmploymentChecks.ToString(),
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

        [Given(@"the apprenticeship incentive has a second earning due for payment")]
        public async Task GivenTheApprenticeshipIncentiveHasASecondEarningDueForPayment()
        {
            var firstPayment = new PendingPayment
            {
                ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id,
                AccountId = _apprenticeshipIncentive.AccountId,
                AccountLegalEntityId = _apprenticeshipIncentive.AccountLegalEntityId,
                Amount = 1500m,
                CalculatedDate = DateTime.Now,
                ClawedBack = false,
                DueDate = _apprenticeshipIncentive.StartDate.AddDays(90),
                EarningType = EarningType.FirstPayment,
                Id = Guid.NewGuid(),
                PaymentMadeDate = _apprenticeshipIncentive.StartDate.AddDays(100),
                PaymentYear = 2122,
                PeriodNumber = 1,
                ValidationResults = new List<PendingPaymentValidationResult>()
            };

            var secondPayment = new PendingPayment
            {
                ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id,
                AccountId = _apprenticeshipIncentive.AccountId,
                AccountLegalEntityId = _apprenticeshipIncentive.AccountLegalEntityId,
                Amount = 1500m,
                CalculatedDate = DateTime.Now,
                ClawedBack = false,
                DueDate = _apprenticeshipIncentive.StartDate.AddDays(365),
                EarningType = EarningType.SecondPayment,
                Id = Guid.NewGuid(),
                PaymentMadeDate = null,
                PaymentYear = 2122,
                PeriodNumber = 12,
                ValidationResults = new List<PendingPaymentValidationResult>()
            };

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(firstPayment);
            await dbConnection.InsertAsync(secondPayment);
        }

        [Given(@"the initial employment checks have not been run")]
        public async Task GivenTheInitialEmploymentChecksHaveNotBeenRun()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentChecks = dbConnection.GetAll<EmploymentCheck>();
            employmentChecks.Count().Should().Be(0);
        }

        [Given(@"the '(.*)' employment check has a result of '(.*)'")]
        public async Task GivenTheEmploymentCheckHasAFailureResult(EmploymentCheckType employmentCheckType, bool result)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = new EmploymentCheck
            {
                ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id,
                CheckType = employmentCheckType,
                CorrelationId = Guid.NewGuid(),
                CreatedDateTime = DateTime.Now,
                MinimumDate = DateTime.Today.AddDays(-60),
                MaximumDate = DateTime.Today,
                Id = Guid.NewGuid(),
                Result = result
            };
            await dbConnection.InsertAsync(employmentCheck);
        }

        [Given(@"the 365 day checks have not previously been processed")]
        public async Task GivenThe365DayChecksHaveNotPreviouslyBeenProcessed()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentChecks = dbConnection.GetAll<EmploymentCheck>();
            employmentChecks.FirstOrDefault(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck).Should().BeNull();
        }

        [When(@"the employment check refresh processing resumes")]
        public async Task WhenEmploymentCheckRefreshProcessingResumes()
        {
            await TestContext.WaitFor<ICommand>(async (cancellationToken) =>
                await TestContext.MessageBus.Send(_delayedCommand), numberOfOnProcessedEventsExpected: 1);
        }

        [When(@"an '(.*)' employment check refresh is requested")]
        public async Task WhenAnEmploymentCheckRefreshIsRequested(string checkType)
        {
            var applications = new List<Application>
            {
                new Application
                {
                    AccountLegalEntityId = _apprenticeshipIncentive.AccountLegalEntityId,
                    ULN = _apprenticeshipIncentive.ULN
                }
            };

            var data = new Dictionary<string, object>
            {
                { "CheckType", checkType },
                { "Applications", JsonConvert.SerializeObject(applications) },
                { "ServiceRequest", JsonConvert.SerializeObject(_serviceRequest) }
            };

            _response = await EmployerIncentiveApi.Put("/jobs",  new JobRequest { Type = JobType.RefreshEmploymentChecks, Data = data });
        }

        [Then(@"a request is made to refresh the employment checks for the incentive")]
        public async Task ThenARequestIsMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Should().Be(2);

            TestContext.CommandsPublished.Count(c => c.IsDomainCommand && c.IsPublished && c.Command is SendEmploymentCheckRequestsCommand command && command.CheckType == Enums.EmploymentCheckType.EmployedBeforeSchemeStarted).Should().Be(1);
            TestContext.CommandsPublished.Count(c => c.IsDomainCommand && c.IsPublished && c.Command is SendEmploymentCheckRequestsCommand command && command.CheckType == Enums.EmploymentCheckType.EmployedAtStartOfApprenticeship).Should().Be(1);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var audits = await dbConnection.GetAllAsync<EmploymentCheckAudit>();
            audits.Should().HaveCount(2);
            
            audits.First().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audits.First().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audits.First().ServiceRequestTaskId.Should().Be(_serviceRequest.TaskId);
            audits.First().ServiceRequestDecisionReference.Should().Be(_serviceRequest.DecisionReference);
            audits.First().ServiceRequestCreatedDate.Should().Be(_serviceRequest.TaskCreatedDate.Value);

            audits.Last().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audits.Last().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audits.Last().ServiceRequestTaskId.Should().Be(_serviceRequest.TaskId);
            audits.Last().ServiceRequestDecisionReference.Should().Be(_serviceRequest.DecisionReference);
            audits.Last().ServiceRequestCreatedDate.Should().Be(_serviceRequest.TaskCreatedDate.Value);
        }

        [Then(@"a request is not made to refresh the employment checks for the incentive")]
        public void ThenARequestIsNotMadeToRefreshTheEmploymentChecksForTheIncentive()
        {
            TestContext.CommandsPublished.Count(c =>
                    c.IsDomainCommand &&
                    c.IsPublished &&
                    c.Command is SendEmploymentCheckRequestsCommand)
                .Should().Be(0);

            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

        [Then(@"a request is made to refresh the 365 day employment check for the incentive")]
        public async Task ThenARequestIsMadeToRefreshThe365DaysEmploymentCheckForTheIncentive()
        {
            TestContext.CommandsPublished.Count(c => c.IsDomainCommand && c.IsPublished && c.Command is SendEmploymentCheckRequestsCommand command && command.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck).Should().Be(1);

            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var audits = await dbConnection.GetAllAsync<EmploymentCheckAudit>();
            audits.Should().HaveCount(1);

            audits.First().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            audits.First().CheckType.Should().Be(EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck.ToString());
            audits.First().ServiceRequestTaskId.Should().Be(_serviceRequest.TaskId);
            audits.First().ServiceRequestDecisionReference.Should().Be(_serviceRequest.DecisionReference);
            audits.First().ServiceRequestCreatedDate.Should().Be(_serviceRequest.TaskCreatedDate.Value);
        }
    }
}
