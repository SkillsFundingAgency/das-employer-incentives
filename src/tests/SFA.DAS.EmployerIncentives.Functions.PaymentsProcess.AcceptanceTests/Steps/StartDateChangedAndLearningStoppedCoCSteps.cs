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
    [Scope(Feature = "StartDateChangedWithLearningStoppedCoC")]
    public class StartDateChangedWithLearningStoppedCoCSteps
    {
        private readonly TestContext _context;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _firstPendingPayment;
        private readonly PendingPayment _secondPendingPayment;
        private readonly DateTime _plannedStartDate;

        public StartDateChangedWithLearningStoppedCoCSteps(TestContext context)
        {
            _context = context;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 8, 1);
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.Phase, Phase.Phase1)
                .Create();

            _firstPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _secondPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(7))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            await using (var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertWithEnumAsStringAsync(_firstPendingPayment);
                await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);                
            }

            await _context.SetActiveCollectionCalendarPeriod(2122, 1);
        }

        [Given(@"the learner data identifies the learner as having stopped CoC and a StartDate CoC")]
        public void GivenTheLearnerDataIdentifiesTheLearnerAsHavingStoppedCocAndAStartDateCoc()
        {
            //
        }

        [Given(@"the stopped date is before the recalculated earning due date")]
        public void GivenStoppedDateIsBeforeTheRecalculatedEarningDueDate()
        {
            var stoppedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
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
                                        .With(period => period.Period, _firstPendingPayment.PeriodNumber)
                                        .Create()
                                })
                                .With(pe => pe.StartDate, _plannedStartDate.AddDays(2))
                                .With(pe => pe.EndDate, _plannedStartDate.AddDays(89 - 1))
                                .Create() }
                        )
                        .Create()}
                )
                .Create();

            SetupMockLearnerMatchResponse(stoppedLearnerMatchApiData);
        }

        [Given(@"the stopped date is between the recalculated earning first and second due dates")]
        public void GivenStoppedDateIsBetweenTheRecalculatedFirstAndSecondDueDates()
        {
            var stoppedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                                _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear,"2021")
                                .With(x => x.StartDate, _plannedStartDate)
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _firstPendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate.AddDays(2))
                            .With(pe => pe.EndDate, _plannedStartDate.AddDays(89 + 2))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            SetupMockLearnerMatchResponse(stoppedLearnerMatchApiData);
        }

        [Given(@"the stopped date is after the recalculated second due date")]
        public void GivenStoppedDateIsAfterTheRecalculatedSecondDueDate()
        {
            var stoppedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                                _fixture.Build<PriceEpisodeDto>()
                                .With(x => x.AcademicYear,"2021")
                                .With(x => x.StartDate, _plannedStartDate)
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _firstPendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, _plannedStartDate.AddDays(2))
                            .With(pe => pe.EndDate, _plannedStartDate.AddDays(364 + 2))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

            SetupMockLearnerMatchResponse(stoppedLearnerMatchApiData);
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            await StartLearnerMatching();
        }

        [Then(@"both recalculated first and second earnings are deleted")]
        public void ThenBothRecalculatedFirstAndSecondEarningsAreDeleted()
        {
            using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(0);
        }

        [Then(@"retain the first earnings")]
        public void ThenTheFirstEarning()
        {
            using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count(e => e.EarningType == EarningType.FirstPayment).Should().Be(1);
        }

        [Then(@"retain the second earnings")]
        public void ThenTheSecondEarning()
        {
            using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count(e => e.EarningType == EarningType.SecondPayment).Should().Be(1);            
        }

        [Then(@"the second earning is deleted")]
        public void ThenTheSecondEarningIsDeleted()
        {
            using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count(e => e.EarningType == EarningType.SecondPayment).Should().Be(0);
        }

        private void SetupMockLearnerMatchResponse(LearnerSubmissionDto learnerMatchApiData)
        {
            _context.LearnerMatchApi.MockServer
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

        private DateTime GetPastEndDate(DateTime endDate)
        {
            using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var academicYears = dbConnection.GetAll<AcademicYear>();

            if (academicYears.Any(x => x.EndDate == endDate))
            {
                return endDate.AddDays(-1);
            }

            return endDate;
        }

        private async Task StartLearnerMatching()
        {
            await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "LearnerMatchingOrchestrator_Start",
                    nameof(LearnerMatchingOrchestratorStart),
                    new Dictionary<string, object>
                    {
                        ["req"] = TestContext.TestRequest($"/api/orchestrators/LearnerMatchingOrchestrator")
                    }
                ));
        }
    }
}
