using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
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
    [Scope(Feature = "LearningStopped")]
    public class LearningStoppedSteps
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly Learner _learner;
        private readonly LearningPeriod _learningPeriod1;
        private readonly LearnerSubmissionDto _stoppedLearnerMatchApiData;
        private readonly LearnerSubmissionDto _resumedLearnerMatchApiData;
        private readonly LearnerSubmissionDto _resumedLearnerWithBreakInLearningMatchApiData;
        private readonly ApprenticeshipBreakInLearning _apprenticeshipBreakinLearning;
        private readonly DateTime _plannedStartDate;
        private readonly DateTime _periodEndDate;
        private readonly int _breakInLearning;

        public LearningStoppedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 8, 1);
            _breakInLearning = 15;
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, new DateTime(2020, 11, 1))
                .With(p => p.Phase, Phase.Phase1_0)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _periodEndDate = DateTime.Today.AddDays(-10);

            _learner = _fixture
                .Build<Learner>()
                .With(p => p.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.ULN, _apprenticeshipIncentive.ULN)
                .With(p => p.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(p => p.LearningFound, true)
                .With(p => p.StartDate, _plannedStartDate.AddDays(-10))
                .Create();

            _learningPeriod1 = _fixture
                .Build<LearningPeriod>()
                .With(p => p.LearnerId, _learner.Id)
                .With(p => p.StartDate, _plannedStartDate.AddDays(-20).AddDays(_breakInLearning * -1))
                .With(p => p.EndDate, _plannedStartDate.AddDays(-10).AddDays(_breakInLearning * -1))
                .Create();

            _stoppedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, _periodEndDate)
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            _resumedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(10))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            _resumedLearnerWithBreakInLearningMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                            _fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate)
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(_breakInLearning * -1))
                            .Create(),
                            _fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, DateTime.Today)
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(10))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            _apprenticeshipBreakinLearning = _fixture
                .Build<ApprenticeshipBreakInLearning>()
                .With(b => b.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(b => b.StartDate, _plannedStartDate.AddDays(_breakInLearning * -1))
                .With(b => b.EndDate, (DateTime?)null)
                .Create();
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_pendingPayment);
            }
        }

        [Given(@"an apprenticeship incentive exists that has stopped learning")]
        public async Task GivenAnApprenticeshipIncentiveExistsThatHasStoppedLearning()
        {
            _apprenticeshipIncentive.Status = IncentiveStatus.Stopped;

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_apprenticeshipBreakinLearning);
                await dbConnection.InsertAsync(_pendingPayment);
                await dbConnection.InsertAsync(_learner);
                await dbConnection.InsertAsync(_learningPeriod1);
            }
        }

        [Given(@"the learner data identifies the learner as not in learning anymore")]
        public void GivenTheIncentiveLearnerDataIdentifiesTheLearnerAsNotInLearningAnymore()
        {
            SetupMockLearnerMatchResponse(_stoppedLearnerMatchApiData);            
        }

        [Given(@"the learner data identifies the learner as in learning")]
        public void GivenTheIncentiveLearnerDataIdentifiesTheLearnerAsInLearning()
        {
            SetupMockLearnerMatchResponse(_resumedLearnerMatchApiData);
        }

        [Given(@"the learner data identifies the learner as in leaning with a break in learning")]
        public void GivenTheIncentiveLearnerDataIdentifiesTheLearnerAsInLearningWithABreakInLearning()
        {
            SetupMockLearnerMatchResponse(_resumedLearnerWithBreakInLearningMatchApiData);
        }

        [Given(@"the apprenticeship incentive has unpaid earnings after the stopped date")]
        public async Task GivenTheApprenticeshipIncentiveHasUnpaidEarningsAfterTheStoppedDate()
        {
            var futurePendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _periodEndDate.AddMonths(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(futurePendingPayment);
        }

        [Given(@"the apprenticeship incentive has paid earnings after the stopped date")]
        public async Task GivenTheApprenticeshipIncentiveHasPaidEarningsAfterTheStoppedDate()
        {
            var paidPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _periodEndDate.AddMonths(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .With(p => p.PaymentMadeDate, DateTime.Now.AddDays(-1))
                .Create();

            var payment = _fixture.Build<Payment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.PaidDate, DateTime.Now.AddDays(-1))
                .With(p => p.PendingPaymentId, paidPendingPayment.Id)
                .With(p => p.PaymentYear, paidPendingPayment.PaymentYear)
                .With(p => p.PaymentPeriod, paidPendingPayment.PeriodNumber)
                .Create();

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(paidPendingPayment);
            await dbConnection.InsertAsync(payment);
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            await StartLearnerMatching();
        }

        [Then(@"the incentive is updated to stopped")]
        public void ThenTheIncentiveIsUpdatedToStopped()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().Status.Should().Be(IncentiveStatus.Stopped);
        }

        [Then(@"the learner data stopped date is stored")]
        public void ThenTheLearnerDataStoppedDateIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>();

            learner.Single().LearningStoppedDate.Should().Be(_periodEndDate.AddDays(1));
            learner.Single().LearningResumedDate.Should().Be(null);
        }

        [Then(@"the learner start break in learning is stored")]
        public void ThenTheLearnerStartBreakInLearningIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breakInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>();

            breakInLearning.Single().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            breakInLearning.Single().StartDate.Should().Be(_periodEndDate.AddDays(1));
            breakInLearning.Single().EndDate.Should().Be(null);
        }

        [Then(@"the learner resume break in learning is stored")]
        public void ThenTheLearnerResumeBreakInLearningIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breakInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>();

            var temp = breakInLearning.Single();
            breakInLearning.Single().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            breakInLearning.Single().StartDate.Should().Be(_plannedStartDate.AddDays(_breakInLearning * -1));
            breakInLearning.Single().EndDate.Should().Be(_plannedStartDate.AddDays(-1));
        }

        [Then(@"the learner data resumed date is stored")]
        public void ThenTheLearnerDataResumedDateIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>();

            learner.Single().LearningResumedDate.Should().Be(_plannedStartDate);
            learner.Single().LearningStoppedDate.Should().Be(null);            
        }

        [Then(@"the incentive is updated to active")]
        public void ThenTheIncentiveIsUpdatedToActive()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().Status.Should().Be(IncentiveStatus.Active);
        }

        [Then(@"the stopped change of circumstance is saved")]
        public void ThenTheStoppedChangeOfCircumstancesIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single(coc=> coc.ChangeType == ChangeOfCircumstanceType.LearningStopped);

            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(string.Empty);
            change.NewValue.Should().Be(_periodEndDate.AddDays(1).ToString("yyyy-MM-dd"));
            change.ChangedDate.Should().Be(DateTime.Today);
        }

        [Then(@"the resumed change of circumstance is saved")]
        public void ThenTheResumedChangeOfCircumstancesIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single(coc => coc.ChangeType == ChangeOfCircumstanceType.LearningResumed);

            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(string.Empty);
            change.NewValue.Should().Be(_plannedStartDate.ToString("yyyy-MM-dd"));
            change.ChangedDate.Should().Be(DateTime.Today);
        }

        [Then(@"the pending payment due dates include the break in learning")]
        public void ThenThePendingPaymentDuedatesIncludeTheBreakInLearning()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Single(p => p.EarningType == EarningType.FirstPayment).DueDate.Should().Be(_plannedStartDate.AddDays(89).AddDays(_breakInLearning - 1));
            pendingPayments.Single(p => p.EarningType == EarningType.SecondPayment).DueDate.Should().Be(_plannedStartDate.AddDays(364).AddDays(_breakInLearning - 1));
        }

        [Then(@"the existing pending payments are removed")]
        public void ThenTheExistingPendingPaymentsAreRemoved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(1);
            pendingPayments.Single().EarningType.Should().Be(EarningType.FirstPayment);
        }

        [Then(@"the existing paid pending payments are clawed back")]
        public void ThenTheExistingPaidPendingPaymentsAreClawedBack()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var pendingPayment = dbConnection.GetAll<PendingPayment>().Single(p => p.EarningType == EarningType.SecondPayment);
            pendingPayment.ClawedBack.Should().BeTrue();
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
