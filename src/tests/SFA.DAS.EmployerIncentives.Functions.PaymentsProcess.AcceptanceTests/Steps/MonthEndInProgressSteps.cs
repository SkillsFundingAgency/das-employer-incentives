using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "MonthEndInProgress")]
    public class MonthEndInProgressSteps
    {
        private TestContext _testContext;
        private Fixture _fixture;

        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        public MonthEndInProgressSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
        }

        [Given(@"the active collection period is not currently in progress")]
        public void GivenTheActiveCollectionPeriodIsNotCurrentlyInProgress()
        {
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            var accountModel = _fixture.Create<Data.Models.Account>();

            var apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.AccountId, accountModel.Id)
                .With(p => p.AccountLegalEntityId, accountModel.AccountLegalEntityId)
                .Without(p => p.PendingPayments)
                .Without(p => p.Payments)
                .Create();

            var pendingPayments = new List<PendingPayment>
            {
                _fixture.Build<PendingPayment>()
                    .With(p => p.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                    .With(p => p.AccountId, apprenticeshipIncentive.AccountId)
                    .With(p => p.AccountLegalEntityId, apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.ClawedBack, false)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .Without(p => p.PaymentMadeDate)
                    .Create(),
            };

            apprenticeshipIncentive.PendingPayments = pendingPayments;

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(accountModel);
            await dbConnection.InsertAsync(apprenticeshipIncentive);
            foreach (var pendingPayment in pendingPayments)
            {
                await dbConnection.InsertAsync(pendingPayment);
            }
        }

        [Given(@"the active collection period is currently in progress")]
        public async Task GivenTheActiveCollectionPeriodIsCurrentlyInProgress()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _testContext.ActivePeriod.PeriodEndInProgress = true;
            await dbConnection.UpdateAsync(_testContext.ActivePeriod);
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "IncentivePaymentOrchestrator_HttpStart",
                    nameof(IncentivePaymentOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/IncentivePaymentOrchestrator/{CollectionPeriodYear}/{CollectionPeriod}"
                        },
                        ["collectionPeriodYear"] = CollectionPeriodYear,
                        ["collectionPeriodNumber"] = CollectionPeriod
                    },
                    expectedCustomStatus: "WaitingForPaymentApproval"
                ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var response = await _testContext.TestFunction.GetOrchestratorStartResponse();
            var status = await _testContext.TestFunction.GetStatus(response.Id);
            status.CustomStatus.ToObject<string>().Should().Be("WaitingForPaymentApproval");
        }

        [When(@"the learner match process is run")]
        public async Task WhenTheLearnerMatchProcessIsRun()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "LearnerMatchingOrchestrator_Start",
                    nameof(LearnerMatchingOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/LearnerMatchingOrchestrator"
                        }
                    }
                ));
        }

        [Then(@"the active collection period is set to in progress")]
        public async Task ThenTheValidationStepWillHaveAFailedValidationResult()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod>().Result.Where(x => x.Active).ToList();
            results.Should().HaveCount(1);
            results.First().PeriodEndInProgress.Should().BeTrue();
        }

        [Then(@"the learner match data is not updated")]
        public void ThenTheLearnerMatchDataIsNotUpdated()
        {
            _testContext.LearnerMatchApi.MockServer.LogEntries.Count().Should().Be(0);
        }
    }
}
