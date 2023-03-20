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
        private PendingPayment _firstPendingPayment;
        private PendingPayment _secondPendingPayment;
        private Payment _firstPayment;
        private Payment _secondPayment;
        private ChangeOfCircumstance _changeOfCircumstance;

        private readonly Learner _learner;
        private readonly LearningPeriod _learningPeriod1;
        private readonly LearnerSubmissionDto _stoppedLearnerMatchApiData;
        private readonly LearnerSubmissionDto _resumedLearnerMatchApiData;
        private readonly LearnerSubmissionDto _resumedLearnerWithBreakInLearningMatchApiData;
        private readonly LearnerSubmissionDto _resumedLearnerWithIncorrectlyRecordedBreakInLearningMatchApiData;
        private readonly LearnerSubmissionDto _updatedStoppedLearnerMatchApiData;
        private readonly ApprenticeshipBreakInLearning _apprenticeshipBreakInLearning;
        private readonly DateTime _plannedStartDate;
        private readonly DateTime _periodEndDate;
        private readonly int _breakInLearning;
        private readonly DateTime _resumedDate;
        private readonly DateTime _stoppedDate;
        private DateTime _updatedStoppedDate;
        private bool _dataIsInitialised = true;

        public LearningStoppedSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 8, 1);
            _breakInLearning = 29;
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
                .With(p => p.Phase, Phase.Phase1)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _periodEndDate = GetPastEndDate(-10);

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
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear,"2021")
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

            _resumedDate = _plannedStartDate.AddDays(100).AddDays(_breakInLearning);
            _stoppedDate = _plannedStartDate.AddDays(100);

            _resumedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto>
                    {
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                                {
                                    _fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId,
                                                    _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber)
                                                .Create()
                                        })
                                        .With(pe => pe.StartDate, _plannedStartDate)
                                        .With(pe => pe.EndDate, _stoppedDate)
                                        .Create(),
                                    _fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId,
                                                    _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber)
                                                .Create()
                                        })
                                        .With(pe => pe.StartDate, _resumedDate)
                                        .Without(pe => pe.EndDate)
                                        .Create()
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            _resumedLearnerWithBreakInLearningMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                            _fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear,"2021")
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
                            _fixture.Build<PriceEpisodeDto>().With(x => x.AcademicYear,"2021")
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

            _apprenticeshipBreakInLearning = _fixture
                .Build<ApprenticeshipBreakInLearning>()
                .With(b => b.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(b => b.StartDate, _plannedStartDate.AddDays(_breakInLearning * -1))
                .With(b => b.EndDate, (DateTime?)null)
                .With(b => b.CreatedDate, DateTime.Today)
                .Without(b => b.UpdatedDate)
                .Create();

            _resumedLearnerWithIncorrectlyRecordedBreakInLearningMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto>
                    {
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear,"2021")
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId,
                                                    _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber)
                                                .Create()
                                        })
                                        .With(pe => pe.StartDate, _apprenticeshipBreakInLearning.StartDate)
                                        .With(pe => pe.EndDate, DateTime.Now.AddMonths(12))
                                        .Create(),
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            var plannedStartDate = new DateTime(2021, 08, 01);
            _updatedStoppedDate = plannedStartDate.AddDays(91);

            _updatedStoppedLearnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, "2021")
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear,"2021")
                            .With(x => x.StartDate, plannedStartDate)
                            .With(x => x.EndDate, plannedStartDate.AddDays(91))
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                    .With(period => period.IsPayable, true)
                                    .With(period => period.Period, _pendingPayment.PeriodNumber)
                                    .Create()
                            })
                            .With(pe => pe.StartDate, plannedStartDate)
                            .With(pe => pe.EndDate, _updatedStoppedDate)
                            .Create() }
                        )
                        .Create()}
                )
                .Create();

        }

        private DateTime GetPastEndDate(int daysFromEndDate)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var academicYears = dbConnection.GetAll<AcademicYear>();

            var collectionPeriods = dbConnection.GetAll<CollectionCalendarPeriod>();
            var activePeriod = collectionPeriods.First(x => x.Active);

            var endDate = activePeriod.CensusDate.AddDays(daysFromEndDate);

            if (academicYears.Any(x => x.EndDate == endDate))
            {
                return endDate.AddDays(-1);
            }

            return endDate;
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertWithEnumAsStringAsync(_pendingPayment);            
        }

        [Given(@"an apprenticeship incentive exists that has stopped learning")]
        public async Task GivenAnApprenticeshipIncentiveExistsThatHasStoppedLearning()
        {
            _apprenticeshipIncentive.Status = IncentiveStatus.Stopped;

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_apprenticeshipBreakInLearning);
                await dbConnection.InsertWithEnumAsStringAsync(_pendingPayment);
                await dbConnection.InsertAsync(_learner);
                await dbConnection.InsertAsync(_learningPeriod1);
            }
        }

        [Given(@"a stopped apprenticeship incentive exists")]
        public void GivenAStoppedApprenticeshipIncentiveExists()
        {
            _apprenticeshipIncentive.StartDate = _plannedStartDate;
            _apprenticeshipIncentive.Status = IncentiveStatus.Stopped;
            _apprenticeshipIncentive.Phase = Phase.Phase1;
            _apprenticeshipIncentive.PendingPayments = new List<PendingPayment>();
            _apprenticeshipIncentive.Payments = new List<Payment>();
            _learner.StartDate = _apprenticeshipIncentive.StartDate;
            _learner.InLearning = false;
            _learner.LearningStoppedDate = _learner.StartDate.Value.AddDays(366);
            _learner.LearningResumedDate = null;
            
            _dataIsInitialised = false;
        }

        [Given(@"the first payment is (.*)")]
        public void GivenTheFirstPaymentIsXWhenTheStopDateIsY(string firstPaymentIsPaid)
        {
            _firstPendingPayment = _fixture.Build<PendingPayment>()
                    .With(p => p.AccountId, _accountModel.Id)
                    .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(p => p.DueDate, _learner.StartDate.Value.AddDays(89))
                    .With(p => p.ClawedBack, false)
                    .With(p => p.EarningType, EarningType.FirstPayment)
                    .With(p => p.Amount, 1000)
                    .Without(p => p.ValidationResults)
                    .Create();

            _firstPendingPayment.PaymentYear = 2021;
            _firstPendingPayment.PeriodNumber = 3;

            if (firstPaymentIsPaid == "not paid")
            {
                _firstPendingPayment.PaymentMadeDate = null;
                _firstPayment = null;
            }
            else if (firstPaymentIsPaid == "paid")
            {
                _firstPendingPayment.PaymentMadeDate = _firstPendingPayment.DueDate;

                _firstPayment = _fixture.Build<Payment>()
                   .With(p => p.AccountId, _accountModel.Id)
                   .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                   .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                   .With(p => p.PaidDate, _firstPendingPayment.PaymentMadeDate)
                   .With(p => p.PendingPaymentId, _firstPendingPayment.Id)
                   .With(p => p.PaymentYear, _firstPendingPayment.PaymentYear)
                   .With(p => p.PaymentPeriod, _firstPendingPayment.PeriodNumber)
                   .Create();
            }
            else
            {
                throw new Exception("Invalid firstPaymentIsPaid");
            }
        }

        [Given(@"the second payment is (.*)")]
        public void GivenTheSecondPaymentIsXWhenTheStopDateIsY(string secondPaymentIsPaid)
        {
            _secondPendingPayment = _fixture.Build<PendingPayment>()
                    .With(p => p.AccountId, _accountModel.Id)
                    .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                    .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(p => p.DueDate, _learner.StartDate.Value.AddDays(364))
                    .With(p => p.ClawedBack, false)
                    .With(p => p.EarningType, EarningType.SecondPayment)
                    .With(p => p.Amount, 1000)
                    .Without(p => p.ValidationResults)
                    .Create();

            _secondPendingPayment.PaymentYear = 2021;
            _secondPendingPayment.PeriodNumber = 12;

            if (secondPaymentIsPaid == "not paid")
            {
                _secondPendingPayment.PaymentMadeDate = null;
                _secondPayment = null;
            }
            else if (secondPaymentIsPaid == "paid")
            {
                _secondPendingPayment.PaymentMadeDate = _learner.StartDate.Value.AddDays(364);

                _secondPayment = _fixture.Build<Payment>()
                   .With(p => p.AccountId, _accountModel.Id)
                   .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                   .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                   .With(p => p.PaidDate, _secondPendingPayment.PaymentMadeDate)
                   .With(p => p.PendingPaymentId, _secondPendingPayment.Id)
                   .With(p => p.PaymentYear, _secondPendingPayment.PaymentYear)
                   .With(p => p.PaymentPeriod, _secondPendingPayment.PeriodNumber)
                   .Create();
            }
            else
            {
                throw new Exception("Invalid secondPaymentIsPaid");
            }
        }

        [Given(@"the stop date is (.*) the first payment and (.*) the second payment")]
        public void GivenTheStopdateIsXTheFirstPaymentAndYTheSecondPayment(string whenFirstPaymentIsPaid, string whenSecondPaymentIsPaid)
        {
            _stoppedLearnerMatchApiData.Training.First().PriceEpisodes.First().EndDate = whenSecondPaymentIsPaid
            switch
            {
                "before" => whenFirstPaymentIsPaid
                switch
                {
                    "before" => _firstPendingPayment.DueDate.AddDays(-2),
                    "on" => _firstPendingPayment.DueDate.AddDays(1),
                    "after" => _firstPendingPayment.DueDate.AddDays(2),
                    _ => throw new Exception("Invalid whenFirstPaymentIsPaid"),
                },
                "on" => _secondPendingPayment.DueDate.AddDays(1),
                "after" => _secondPendingPayment.DueDate.AddDays(2),
                _ => throw new Exception("Invalid whenSecondPaymentIsPaid"),
            };
        }

        [Given(@"the learner data identifies the learning stopped date has changed")]
        public async Task GivenTheIlrStoppedDateHasChanged()
        {
            await ClearActiveCollectionCalendar();

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<CollectionCalendarPeriod>();
            var period = calendar.Single(p => p.CalendarYear == 2021 && p.CalendarMonth == 11);
            period.Active = true;
            await dbConnection.UpdateAsync(period);

            SetupMockLearnerMatchResponse(_stoppedLearnerMatchApiData);
        }

        [Given(@"an apprenticeship incentive exists that has stopped learning before the first payment is due")]
        public async Task GivenAnApprenticeshipIncentiveExistsThatHasStoppedLearningBeforePaymentIsDue()
        {
            _apprenticeshipIncentive.StartDate = new DateTime(2021, 08, 31);
            _apprenticeshipIncentive.Status = IncentiveStatus.Stopped;
            _apprenticeshipIncentive.Phase = Phase.Phase2;
            _learner.StartDate = new DateTime(2021, 08, 01);
            _learner.InLearning = false;
            _learner.LearningStoppedDate = _learner.StartDate.Value.AddDays(88);
            _learner.LearningResumedDate = null;

            _changeOfCircumstance = new ChangeOfCircumstance
            {
                ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id,
                ChangeType = ChangeOfCircumstanceType.LearningStopped,
                ChangedDate = DateTime.Today.AddDays(-1),
                Id = Guid.NewGuid(),
                PreviousValue = string.Empty,
                NewValue = _learner.LearningStoppedDate.Value.ToString("yyyy-MM-dd")
            };

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertAsync(_learner);
                await dbConnection.InsertAsync(_changeOfCircumstance);
                await dbConnection.InsertAsync(_learningPeriod1);
            }
        }

        [Given(@"the first payment has previously been clawed back")]
        public async Task GivenTheFirstPaymentHasPreviouslyBeenClawedBack()
        {
            _pendingPayment.ClawedBack = true;
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_pendingPayment);
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

        [Given(@"the learner data identifies the learner as in learning with start date before recorded break date")]
        public void GivenTheLearnerDataIdentifiesTheLearnerAsInLearningWithStartDateBeforeRecordedBreakDate()
        {
            SetupMockLearnerMatchResponse(_resumedLearnerWithIncorrectlyRecordedBreakInLearningMatchApiData);
        }

        [Given(@"the learner data identifies the learning stopped date has changed to a date after the first payment is due")]
        public async Task GivenTheLearnerDataIdentifiesTheLearningStoppedDateHasChangedToADateAfterAPaymentIsDue()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<CollectionCalendarPeriod>();
            var previousPeriod = calendar.Single(x => x.CalendarYear == 2020 && x.CalendarMonth == 8);
            previousPeriod.Active = false;
            previousPeriod.PeriodEndInProgress = false;

            var currentPeriod = calendar.Single(x => x.CalendarYear == 2021 && x.CalendarMonth == 11);
            currentPeriod.Active = true;
            currentPeriod.PeriodEndInProgress = false;

            await dbConnection.UpdateAsync(previousPeriod);
            await dbConnection.UpdateAsync(currentPeriod);

            _testContext.ActivePeriod = currentPeriod;

            SetupMockLearnerMatchResponse(_updatedStoppedLearnerMatchApiData);
        }

        [Then(@"the most recent break in learning record is deleted")]
        public void ThenTheMostRecentBreakInLearningRecordIsDeleted()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breakInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>();
            breakInLearning.Should().BeEmpty();
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
            await dbConnection.InsertWithEnumAsStringAsync(paidPendingPayment);
            await dbConnection.InsertAsync(payment);
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            if (!_dataIsInitialised)
            {
                await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                if (_firstPendingPayment != null)
                {
                    await dbConnection.InsertWithEnumAsStringAsync(_firstPendingPayment);
                }
                if (_secondPendingPayment != null)
                {
                    await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);
                }
                if (_firstPayment != null)
                {
                    await dbConnection.InsertWithEnumAsStringAsync(_firstPayment);
                }
                if (_secondPayment != null)
                {
                    await dbConnection.InsertWithEnumAsStringAsync(_secondPayment);
                }
            }

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
            breakInLearning.Single().CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
            breakInLearning.Single().UpdatedDate.Should().BeNull();

        }

        [Then(@"the learner start break in learning for the change in stopped date is stored")]
        public void ThenTheLearnerStartBreakInLearningForTheChangeInStoppedDateIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breakInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>();

            breakInLearning.Single().ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            breakInLearning.Single().StartDate.Should().Be(_updatedStoppedDate.AddDays(1));
            breakInLearning.Single().EndDate.Should().Be(null);
            breakInLearning.Single().CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
            breakInLearning.Single().UpdatedDate.Should().BeNull();

        }

        [Then(@"the learner resume break in learning is stored")]
        public void ThenTheLearnerResumeBreakInLearningIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breakInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

            breakInLearning.StartDate.Should().Be(_stoppedDate.AddDays(1));
            breakInLearning.EndDate.Should().Be(_resumedDate.AddDays(-1));
            breakInLearning.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
            breakInLearning.UpdatedDate.Should().BeNull();
        }

        [Then(@"the learner data resumed date is stored")]
        public void ThenTheLearnerDataResumedDateIsStored()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>();

            learner.Single().LearningResumedDate.Should().Be(_resumedDate);
            learner.Single().LearningStoppedDate.Should().Be(null);
        }

        [Then(@"the learner data stopped and resumed dates are deleted")]
        public void ThenTheLearnerDataStoppedAndResumedDatesAreDeleted()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>();

            learner.Single().LearningStoppedDate.Should().BeNull();
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
            change.NewValue.Should().Be(_resumedDate.ToString("yyyy-MM-dd"));
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

            pendingPayments.Count().Should().Be(0);
        }

        [Then(@"the existing paid pending payments are clawed back")]
        public void ThenTheExistingPaidPendingPaymentsAreClawedBack()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var pendingPayment = dbConnection.GetAll<PendingPayment>().Single(p => p.EarningType == EarningType.SecondPayment && p.ClawedBack);
            pendingPayment.ClawedBack.Should().BeTrue();
        }

        [Then(@"the first payment is recalculated")]
        public void ThenTheFirstPaymentIsRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(1);
            pendingPayments.Single().EarningType.Should().Be(EarningType.FirstPayment);
        }

        [Then(@"the first earning is (.*)")]
        public async Task ThenTheFirstEarningIsX(string earningState)
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var pendingPayment = dbConnection.GetAll<PendingPayment>().SingleOrDefault(p => p.EarningType == EarningType.FirstPayment);

            switch (earningState)
            {
                case "retained":
                    pendingPayment.Should().BeEquivalentTo(_firstPendingPayment, opt => opt.Excluding(i => i.CalculatedDate));
                    pendingPayment.CalculatedDate.Should().BeCloseTo(_firstPendingPayment.CalculatedDate, new TimeSpan(0, 1, 0));
                    if (_firstPayment != null)
                    {
                        var payment = dbConnection.GetAll<Payment>().SingleOrDefault(p => p.PendingPaymentId == _firstPendingPayment.Id);
                        payment.Should().NotBeNull();
                    }
                    var clawbackPayment = dbConnection.GetAll<ClawbackPayment>().SingleOrDefault(c => c.PendingPaymentId == _firstPendingPayment.Id);
                    clawbackPayment.Should().BeNull();
                    break;

                case "clawedback":
                    pendingPayment.ClawedBack.Should().BeTrue();
                    var clawbackPayment2 = dbConnection.GetAll<ClawbackPayment>().SingleOrDefault(c => c.PendingPaymentId == _firstPendingPayment.Id);
                    clawbackPayment2.Should().NotBeNull();
                    break;

                case "deleted":
                    pendingPayment.Should().BeNull();
                    _firstPayment.Should().BeNull();
                    dbConnection.GetAll<Payment>().SingleOrDefault(p => p.PendingPaymentId == _firstPendingPayment.Id).Should().BeNull();
                    break;

                default:
                    throw new Exception("Invalid first earningState");
            }
        }

        [Then(@"the second earning is (.*)")]
        public async Task ThenTheSecondEarningIsX(string earningState)
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var pendingPayment = dbConnection.GetAll<PendingPayment>().SingleOrDefault(p => p.EarningType == EarningType.SecondPayment);

            switch (earningState)
            {
                case "retained":
                    pendingPayment.Should().BeEquivalentTo(_secondPendingPayment, opt => opt.Excluding(i => i.CalculatedDate));
                    pendingPayment.CalculatedDate.Should().BeCloseTo(_secondPendingPayment.CalculatedDate, new TimeSpan(0, 1, 0));
                    if (_secondPayment != null)
                    {
                        var payment = dbConnection.GetAll<Payment>().SingleOrDefault(p => p.PendingPaymentId == _secondPendingPayment.Id);
                        payment.Should().NotBeNull();
                    }
                    var clawbackPayment = dbConnection.GetAll<ClawbackPayment>().SingleOrDefault(c => c.PendingPaymentId == _secondPendingPayment.Id);
                    clawbackPayment.Should().BeNull();
                    break;

                case "clawedback":
                    pendingPayment.ClawedBack.Should().BeTrue();
                    var clawbackPayment2 = dbConnection.GetAll<ClawbackPayment>().SingleOrDefault(c => c.PendingPaymentId == _secondPendingPayment.Id);
                    clawbackPayment2.Should().NotBeNull();
                    break;

                case "deleted":
                    pendingPayment.Should().BeNull();
                    _secondPayment.Should().BeNull();
                    dbConnection.GetAll<Payment>().SingleOrDefault(p => p.PendingPaymentId == _secondPendingPayment.Id).Should().BeNull();
                    break;

                default:
                    throw new Exception("Invalid second earningState");
            }
        }

        [Then(@"the previously clawed back payment is recalculated")]
        public void ThenThePreviouslyClawedBackPaymentIsRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(2);
            pendingPayments.Count(x => x.EarningType == EarningType.FirstPayment && x.ClawedBack).Should().Be(1);
            pendingPayments.Count(x => x.EarningType == EarningType.FirstPayment && !x.ClawedBack).Should().Be(1);
        }

        [Then(@"the stopped change of circumstance is updated")]
        public void ThenTheStoppedChangeOfCircumstanceIsUpdated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var changesOfCircumstance = dbConnection.GetAll<ChangeOfCircumstance>().Where(coc => coc.ChangeType == ChangeOfCircumstanceType.LearningStopped).OrderByDescending(x => x.NewValue);

            var updatedStoppedDate = _updatedStoppedLearnerMatchApiData.Training.First().PriceEpisodes.First().EndDate.Value.AddDays(1);

            var change = changesOfCircumstance.First();
            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(_changeOfCircumstance.NewValue);
            change.NewValue.Should().Be(updatedStoppedDate.ToString("yyyy-MM-dd"));
            change.ChangedDate.Should().Be(DateTime.Today);
        }

        private async Task ClearActiveCollectionCalendar()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var calendar = await dbConnection.GetAllAsync<CollectionCalendarPeriod>();

            var activePeriods = calendar.Where(c => c.Active);

            foreach (var period in activePeriods)
            {
                period.Active = false;
                period.PeriodEndInProgress = false;
                await dbConnection.UpdateAsync(period);
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
