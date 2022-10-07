using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmploymentCheck")]
    public class EmploymentCheckSteps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly Account _accountModel;
        private DateTime _plannedStartDate;
        private DateTime? _periodEndDate;
        private readonly PendingPayment _pendingPayment;
        private PendingPayment _secondPendingPayment;
        private LearnerSubmissionDto _learnerMatchApiData;

        public EmploymentCheckSteps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _plannedStartDate = new DateTime(2020, 8, 1);
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, _plannedStartDate.AddYears(-24).AddMonths(-10)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, _plannedStartDate)
                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
                .With(p => p.Phase, Phase.Phase1)
                .Without(x => x.EmploymentChecks)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(1))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();
        }

        private LearnerSubmissionDto CreateLearnerMatchApiData()
        {
            return _fixture
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
                            .With(pe => pe.EndDate, _periodEndDate.HasValue ? _periodEndDate.Value : _plannedStartDate.AddYears(3))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();
        }


            [Given(@"an apprenticeship incentive has been submitted in phase '(.*)'")]
        public async Task GivenAnApprenticeshipIncentiveExists(Phase phase)
        {
            switch(phase)
            {
                case Phase.Phase1:
                    _plannedStartDate = new DateTime(2020, 8, 1);
                    break;

                case Phase.Phase2:
                    _plannedStartDate = new DateTime(2021, 8, 1);
                    break;

                case Phase.Phase3:
                    _plannedStartDate = new DateTime(2021, 11, 1);
                    break;
            }
            _plannedStartDate = new DateTime(2020, 8, 1);
            _pendingPayment.DueDate = _plannedStartDate.AddDays(1);
            _apprenticeshipIncentive.StartDate = _plannedStartDate;
            _apprenticeshipIncentive.Phase = phase;
            await CreateIncentive();
        }        
        private async Task CreateIncentive()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.InsertAsync(_accountModel);
                await dbConnection.InsertAsync(_apprenticeshipIncentive);
                await dbConnection.InsertWithEnumAsStringAsync(_pendingPayment);
            }
        }

        [Given(@"we have not previously requested an employment check for the learner")]
        public void GivenWeHaveNotPreviouslyRequestedAnEmploymentCheckForTheLearner()
        {
            // empty
        }

        [Given(@"6 weeks has elapsed since the start date of the apprenticeship")]
        public void GivenSixWeeksHasElapsedSinceTheStartDateOfTheApprenticeship()
        {
            // empty
        }

        [Given(@"3 weeks has elapsed since 365 days after the due date of the second payment")]
        public async Task GivenTHreeWeeksHasElapsedSince365DaysAfterTheDueDateOfTheSecondPayment()
        {
            _testContext.DateTimeService.SetUtcNow(_plannedStartDate.AddDays(365).AddDays(21));

            _secondPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(365))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);
        }

        [Given(@"3 weeks has elapsed since 365 days after the due date of the second payment which was previously paid")]
        public async Task GivenThreeWeeksHasElapsedSince365DaysAfterTheDueDateOfTheSecondPaymentWhichWasPreviouslyPaid()
        {
            _testContext.DateTimeService.SetUtcNow(_plannedStartDate.AddDays(365).AddDays(21));

            _secondPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(365))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .With(p => p.PaymentMadeDate, _plannedStartDate.AddDays(365))
                .Create();

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);
        }


        [Given(@"6 weeks has elapsed since 365 days after the due date of the second payment")]
        public async Task GivenSixWeeksHasElapsedSince365DaysAfterTheDueDateOfTheSecondPayment()
        {
            _testContext.DateTimeService.SetUtcNow(_plannedStartDate.AddDays(365).AddDays(42));

            _secondPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddDays(365))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);
        }

        [Given(@"we have previously requested an employment check for the learner")]
        public async Task GivenWeHavePreviouslyRequestedAnEmploymentCheckForTheLearner()
        {
            var employmentChecks = _fixture.Build<EmploymentCheck>().With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id).CreateMany();

            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                foreach (var employmentCheck in employmentChecks)
                {
                    await dbConnection.InsertWithEnumAsStringAsync(employmentCheck);
                }
            }
        }

        [Given(@"start of apprenticeship employment checks have passed")]
        public async Task GivenStartOfApprenticeshipEmploymentChecksHavePassed()
        {
            var employmentChecks = new List<EmploymentCheck>()
            {
                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(x => x.Result, true)
                    .Create(),

                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(x => x.Result, false)
                    .Create(),
            };

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            foreach (var employmentCheck in employmentChecks)
            {
                await dbConnection.InsertWithEnumAsStringAsync(employmentCheck);
            }
        }

        [Given(@"the learner data identifies the learner as not in learning anymore")]
        public void GivenTheIncentiveLearnerDataIdentifiesTheLearnerAsNotInLearningAnymore()
        {
            _periodEndDate = GetPastEndDate(-10);
        }

        [Given(@"the initial 365 check has failed")]
        public async Task GiventheInitial365CheckHasFailed()
        {
            var employmentChecks = new List<EmploymentCheck>()
            {
                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(x => x.MinimumDate, _secondPendingPayment.DueDate)
                    .With(x => x.MaximumDate, _testContext.DateTimeService.UtcNow().Date)
                    .With(x => x.Result, false)
                    .Create(),
            };

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            foreach (var employmentCheck in employmentChecks)
            {
                await dbConnection.InsertWithEnumAsStringAsync(employmentCheck);
            }
        }

        [Given(@"the initial 365 check has passed")]
        public async Task GiventheInitial365CheckHasPasses()
        {
            var employmentChecks = new List<EmploymentCheck>()
            {
                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                    .With(x => x.MinimumDate, _secondPendingPayment.DueDate)
                    .With(x => x.MaximumDate, _testContext.DateTimeService.UtcNow().Date)
                    .With(x => x.Result, true)
                    .Create(),
            };

            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            foreach (var employmentCheck in employmentChecks)
            {
                await dbConnection.InsertWithEnumAsStringAsync(employmentCheck);
            }
        }

        [Given(@"the learner data is updated with new valid start date for phase '(.*)'")]
        public void GivenTheLearnerIsUpdatedWithANewPhase1StartDate(Phase phase)
        {
            switch (phase)
            {
                case Phase.Phase1:
                    _plannedStartDate = new DateTime(2020, 9, 1);
                    break;

                case Phase.Phase2:
                    _plannedStartDate = new DateTime(2021, 9, 1);
                    break;

                case Phase.Phase3:
                    _plannedStartDate = new DateTime(2021, 12, 01);
                    break;
            }            
        }

        [When(@"an ILR submission is received for that learner")]
        public Task WhenTheIlrSubmissionIsReceived()
        {
            _learnerMatchApiData = CreateLearnerMatchApiData();

            SetupMockLearnerMatchResponse(_learnerMatchApiData);
            return StartLearnerMatching();
        }

        [Then(@"a new employment check is requested to ensure the apprentice was not employed in the 6 months prior to phase '(.*)' starting")]
        public void ThenNotEmployedPriorToPhaseStartEmploymentCheckIsCreatedForPhase1(Phase phase)
        {
            switch (phase)
            {
                case Phase.Phase1:
                    VerifyEmployedPriorToPhaseCheck(new DateTime(2020, 08, 01).AddMonths(-6), new DateTime(2020, 07, 31));
                    break;

                case Phase.Phase2:
                    VerifyEmployedPriorToPhaseCheck(new DateTime(2021, 4, 1).AddMonths(-6), new DateTime(2021, 3, 31));
                    break;

                case Phase.Phase3:
                    VerifyEmployedPriorToPhaseCheck(new DateTime(2021, 10, 1).AddMonths(-6), new DateTime(2021, 09, 30));
                    break;
            }
        }


        [Then(@"a new 365 employment check is requested to ensure the apprentice was employed when the second payment was due for the phase '(.*)'")]
        public void Then365EmploymentCheckOnSecondPaymentForPhase(Phase phase)
        {
            VerifyEmployedAtSecondPayment(_secondPendingPayment.DueDate.Date, _secondPendingPayment.DueDate.AddDays(21).Date);
        }

        [Then(@"the incentive is updated to stopped")]
        public void ThenTheIncentiveIsUpdatedToStopped()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().Status.Should().Be(IncentiveStatus.Stopped);
        }

        [Then(@"the incentive is updated to active")]
        public void ThenTheIncentiveIsUpdatedToActive()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().Status.Should().Be(IncentiveStatus.Active);
        }

        [Then(@"the second payment due date is updated")]
        public void ThenTheSecondPaymentDueDateIsUpdated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var payment = dbConnection.GetAll<PendingPayment>();

            _secondPendingPayment.DueDate = payment.Single(p => p.EarningType == EarningType.SecondPayment).DueDate;            
        }

        [Then(@"a re-request 365 employment check is not requested for the phase '(.*)'")]
        public void ThenAReRequest365EmploymentCheckOnSecondPaymentIsNotRequestedForPhase(Phase phase = Phase.NotSet)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .SingleOrDefault(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);

            employmentCheck.Should().BeNull();
        }        

        [Then(@"a re-request 365 employment check is requested for the phase '(.*)'")]
        public void ThenAReRequest365EmploymentCheckOnSecondPaymentIsRequestedForPhase(Phase phase = Phase.NotSet)
        {
            VerifyEmployedAtSecondPaymentRecheck(_secondPendingPayment.DueDate.Date, _secondPendingPayment.DueDate.AddDays(42).Date);
        }

        [Then(@"a 365 employment check is not requested")]
        public void Then365EmploymentCheckIsNotRequested()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .SingleOrDefault(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);

            employmentCheck.Should().BeNull();
        }

        private void VerifyEmployedAtSecondPayment(DateTime minDate, DateTime maxDate, bool? expectedResult = null)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .Single(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);
            employmentCheck.MinimumDate.Should().Be(minDate);
            employmentCheck.MaximumDate.Should().Be(maxDate);            

            if(expectedResult.HasValue)
            {
                employmentCheck.Result.Should().Be(expectedResult.Value);
            }
            else
            {
                employmentCheck.Result.Should().BeNull();
            }
        }

        private void VerifyEmployedAtSecondPaymentRecheck(DateTime minDate, DateTime maxDate, bool? expectedResult = null)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .Single(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);
            employmentCheck.MinimumDate.Should().Be(minDate);
            employmentCheck.MaximumDate.Should().Be(maxDate);

            if (expectedResult.HasValue)
            {
                employmentCheck.Result.Should().Be(expectedResult.Value);
            }
            else
            {
                employmentCheck.Result.Should().BeNull();
            }
        }

        private void VerifyEmployedPriorToPhaseCheck(DateTime minDate, DateTime maxDate)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>()
                .Single(x => x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);
            employmentCheck.MinimumDate.Should().Be(minDate);
            employmentCheck.MaximumDate.Should().Be(maxDate);
            employmentCheck.Result.Should().BeNull();
        }

        [Then(@"a new employment check is requested to ensure the apprentice was employed in the six weeks following their start date")]
        public void ThenEmployedPriorAfterStartDateEmploymentCheckIsCreated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var employmentCheck = dbConnection.GetAll<EmploymentCheck>().Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);
            employmentCheck.MinimumDate.Should().Be(_plannedStartDate);
            employmentCheck.MaximumDate.Should().Be(_plannedStartDate.AddDays(42));
            employmentCheck.Result.Should().BeNull();
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
