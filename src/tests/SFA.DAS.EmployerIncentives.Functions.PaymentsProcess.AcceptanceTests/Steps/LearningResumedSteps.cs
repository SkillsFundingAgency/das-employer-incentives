using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "LearningResumed")]
    public class LearningResumedSteps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly Account _accountModel;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly ApprenticeshipBreakInLearning _apprenticeshipBreakInLearning;
        private LearnerSubmissionDto _resumedLearnerMatchApiData;

        private readonly Payment _payment;
        private readonly ClawbackPayment _clawbackPayment;
        private readonly PendingPayment _pendingPayment;

        private readonly DateTime _plannedStartDate;
        private DateTime _stoppedDate;
        private readonly int _breakInLearning;
        private readonly byte _periodNumber;

        public LearningResumedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 11, 10);
            _breakInLearning = 31;
            _accountModel = _fixture.Create<Account>();
            _periodNumber = 1;

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, new DateTime(1995, 10, 15)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.PausePayments, false)
                .With(p => p.Status, IncentiveStatus.Stopped)
                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(p => p.Phase, Phase.Phase1)
                .Create();

            _apprenticeshipBreakInLearning = _fixture
                .Build<ApprenticeshipBreakInLearning>()
                .With(b => b.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(b => b.StartDate, new DateTime(2021, 02, 08))
                .With(b => b.EndDate, (DateTime?)null)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, new DateTime(2021, 02, 07))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .With(p => p.PaymentYear, (short)2021)
                .With(p => p.PeriodNumber, (byte)7)
                .With(p => p.Amount, 750)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _payment = _fixture.Build<Payment>()
                    .With(d => d.PendingPaymentId, _pendingPayment.Id)
                    .With(d => d.AccountId, _accountModel.Id)
                    .With(d => d.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                    .With(d => d.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(d => d.PaymentPeriod, _pendingPayment.PeriodNumber)
                    .With(d => d.PaymentYear, _pendingPayment.PaymentYear)
                    .With(d => d.PaidDate, _pendingPayment.DueDate)
                    .With(d => d.Amount, _pendingPayment.Amount)
                    .Create();

            _clawbackPayment = _fixture.Build<ClawbackPayment>()
                    .With(d => d.PendingPaymentId, _pendingPayment.Id)
                    .With(d => d.PaymentId, _payment.Id)
                    .With(d => d.DateClawbackSent, _pendingPayment.DueDate.AddDays(5))
                    .With(d => d.AccountId, _accountModel.Id)
                    .With(d => d.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                    .With(d => d.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(d => d.CollectionPeriod, _pendingPayment.PeriodNumber)
                    .With(d => d.CollectionPeriodYear, _pendingPayment.PaymentYear)
                    .Create();
        }

        [Given(@"an apprenticeship incentive in a stopped state exists")]
        public async Task GivenAnApprenticeshipIncentiveInAStoppedStateExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
            }
        }

        [Given(@"the stopped date was '(.*)' the original first payment due date")]
        public async Task GivenTheStoppedDateIsBeforeOnAfterTheOriginalFirstPaymentDueDate(string whenStopped)
        {
            switch (whenStopped)
            {
                case "Before":
                    _stoppedDate = _pendingPayment.DueDate.AddDays(-1);
                    break;
                case "On":
                    _stoppedDate = _pendingPayment.DueDate;
                    break;
                case "After":
                    _stoppedDate = _pendingPayment.DueDate.AddDays(1);
                    break;
                default:
                    Assert.Fail();
                    break;
            }

            _apprenticeshipBreakInLearning.StartDate = _stoppedDate.AddDays(1);

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_apprenticeshipBreakInLearning);
            }

            _resumedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                            _fixture.Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear,"2021")
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _periodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, _stoppedDate)
                            .Create(),
                            _fixture.Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear,"2021")
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _periodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _stoppedDate.AddDays(_breakInLearning))
                            .With(pe => pe.EndDate, (DateTime?)null)
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            SetupMockLearnerMatchResponse(_resumedLearnerMatchApiData);
        }

        [Given(@"there are '(.*)' first earnings")]
        public async Task GivenThereAreNoUnpaidPaidClawedBackFirstEarnings(string firstPendingPayment)
        {
            switch (firstPendingPayment)
            {
                case "No":
                    return;
                case "Unpaid":
                    await SetUpUnpaidFirstPayment();
                    break;
                case "ClawedBack":
                    await SetUpClawedBackFirstPayment();
                    break;
                case "Paid":
                    await SetUpPaidFirstPayment();
                    break;
                default:
                    Assert.Fail();
                    break;
            }
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            await StartLearnerMatching();
        }

        [Then(@"the incentive is updated to active")]
        public void ThenTheIncentiveIsUpdatedToActive()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().Status.Should().Be(IncentiveStatus.Active);
        }

        [Then(@"the resumed change of circumstance is saved")]
        public void ThenTheResumedChangeOfCircumstancesIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single(coc => coc.ChangeType == ChangeOfCircumstanceType.LearningResumed);

            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(string.Empty);
            change.NewValue.Should().Be(_stoppedDate.AddDays(_breakInLearning).ToString("yyyy-MM-dd"));
            change.ChangedDate.Should().Be(DateTime.Today);
        }

        [Then(@"the break in learning deleted change of circumstance is saved")]
        public void ThenTheBreakInLearningDeletedChangeOfCircumstancesIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single(coc => coc.ChangeType == ChangeOfCircumstanceType.BreakInLearningDel);

            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(string.Empty);
            change.NewValue.Should().Be(string.Empty);
            change.ChangedDate.Should().Be(DateTime.Today);
        }        

        [Then(@"the learner data resumed date is stored")]
        public void ThenTheLearnerDataResumedDateIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>();

            learner.Single().LearningResumedDate.Should().Be(_stoppedDate.AddDays(_breakInLearning));
            learner.Single().LearningStoppedDate.Should().BeNull();
        }

        [Then(@"the first payment due date is '(.*)' to include the break in learning")]
        public void ThenTheFirstPaymentDueDateIsCalculatedToIncludeTHeBreakInLearning(string isCalculated)
        {
            switch (isCalculated)
            {
                case "Calculated":
                    ThenThePendingPaymentDueDateIncludesTheBreakInLearning();
                    return;
                case "NotCalculated":
                    ThenThePendingPaymentDueDateIsNotChanged();
                    break;
                default:
                    Assert.Fail();
                    break;
            }
        }

        private void ThenThePendingPaymentDueDateIncludesTheBreakInLearning()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Single(p => p.EarningType == EarningType.FirstPayment && !p.ClawedBack).DueDate.Should().Be(_apprenticeshipIncentive.StartDate.AddDays(89).AddDays(_breakInLearning - 2));
            pendingPayments.Single(p => p.EarningType == EarningType.SecondPayment).DueDate.Should().Be(_apprenticeshipIncentive.StartDate.AddDays(364).AddDays(_breakInLearning - 2));
        }

        private void ThenThePendingPaymentDueDateIsNotChanged()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Single(p => p.EarningType == EarningType.FirstPayment && !p.ClawedBack).DueDate.Should().Be(_apprenticeshipIncentive.StartDate.AddDays(89));
            pendingPayments.Single(p => p.EarningType == EarningType.SecondPayment).DueDate.Should().Be(_apprenticeshipIncentive.StartDate.AddDays(364).AddDays(_breakInLearning - 2));
        }

        private async Task SetUpUnpaidFirstPayment()
        {
            _pendingPayment.ClawedBack = false;
            _pendingPayment.PaymentMadeDate = null;

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_pendingPayment);
            }
        }

        private async Task SetUpClawedBackFirstPayment()
        {
            _pendingPayment.ClawedBack = true;
            _pendingPayment.PaymentMadeDate = _payment.PaidDate;

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_pendingPayment);
                await dbConnection.InsertAsync(_payment);
                await dbConnection.InsertAsync(_clawbackPayment);
            }
        }

        private async Task SetUpPaidFirstPayment()
        {
            _pendingPayment.PaymentMadeDate = _payment.PaidDate;

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_pendingPayment);
                await dbConnection.InsertAsync(_payment);
            }
        }
        private void SetupMockLearnerMatchResponse(LearnerSubmissionDto learnerMatchApiData)
        {
            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(learnerMatchApiData));
        }

        private async Task StartLearnerMatching()
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
    }
}
