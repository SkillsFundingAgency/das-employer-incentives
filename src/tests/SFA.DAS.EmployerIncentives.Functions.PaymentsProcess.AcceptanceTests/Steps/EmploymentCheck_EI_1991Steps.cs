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
    [Scope(Feature = "EmploymentCheckEI1991")]
    public class EmploymentCheckEI_1991_Steps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly Account _accountModel;
        private readonly PendingPayment _firstPendingPayment;
        private PendingPayment _secondPendingPayment;        
        private readonly Learner _learner;
        private readonly LearningPeriod _learningPeriod;
        private readonly ApprenticeshipDaysInLearning _apprenticeshipDaysInLearning1;
        private readonly ApprenticeshipDaysInLearning _apprenticeshipDaysInLearning2;
        private readonly ApprenticeshipDaysInLearning _apprenticeshipDaysInLearning3;
        private readonly EmploymentCheck _employmentCheck1;
        private readonly EmploymentCheck _employmentCheck2;
        private readonly EmploymentCheck _employmentCheck3;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult1;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult2;
        private readonly PendingPaymentValidationResult _pendingPaymentValidationResult3;
        private readonly Payment _payment1;
        private LearnerSubmissionDto _learnerMatchApiData;        

        public EmploymentCheckEI_1991_Steps(TestContext testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();

            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, new DateTime(2000, 04, 14))
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.StartDate, new DateTime(2021, 10, 01))
                .With(p => p.SubmittedDate, new DateTime(2022, 08, 01))
                .With(p => p.Phase, Phase.Phase3)
                .With(p => p.EmployerType, Common.Domain.Types.ApprenticeshipEmployerType.Levy)
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.PausePayments, false)
                .Without(x => x.EmploymentChecks)
                .Create();

            _firstPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, new DateTime(2022, 08, 22))
                .With(p => p.PaymentMadeDate, new DateTime(2022, 11, 16))
                .With(p => p.PeriodNumber, (byte)1)
                .With(p => p.PaymentYear, (short)2223)
                .With(p => p.CalculatedDate, new DateTime(2022, 08, 01))
                .With(p => p.Amount, 1500)
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Create();

            _secondPendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, new DateTime(2022, 09, 30))
                .Without(p => p.PaymentMadeDate)
                .With(p => p.PeriodNumber, (byte)2)
                .With(p => p.PaymentYear, (short)2223)
                .With(p => p.CalculatedDate, new DateTime(2022, 08, 01))
                .With(p => p.Amount, 1500)
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.SecondPayment)
                .Create();

            _learner = _fixture.Build<Learner>()
                .With(l => l.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(l => l.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                .With(l => l.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(l => l.ULN, _apprenticeshipIncentive.ULN)
                .With(l => l.SubmissionFound, true)
                .With(l => l.SubmissionDate, new DateTime(2022, 09, 16))
                .With(l => l.LearningFound, true)
                .With(l => l.HasDataLock, false)
                .With(l => l.StartDate, new DateTime(2021, 10, 01))
                .Without(l => l.LearningStoppedDate)
                .With(l => l.LearningResumedDate, new DateTime(2022, 08, 01))
                .Create();

            _learningPeriod = _fixture.Build<LearningPeriod>()
                .With(l => l.LearnerId, _learner.Id)
                .With(l => l.StartDate, new DateTime(2021, 10, 01))
                .With(l => l.EndDate, new DateTime(2023, 07, 31))
                .With(l => l.CreatedDate, new DateTime(2022, 10, 25))
                .Create();

            _apprenticeshipDaysInLearning1 = _fixture.Build<ApprenticeshipDaysInLearning>()
                .With(l => l.LearnerId, _learner.Id)
                .With(l => l.NumberOfDaysInLearning, 335)
                .With(l => l.CollectionPeriodNumber, 1)
                .With(l => l.CollectionPeriodYear, 2223)
                .With(l => l.CreatedDate, new DateTime(2022, 10, 25))
                .Without(l => l.UpdatedDate)
                .Create();

            _apprenticeshipDaysInLearning2 = _fixture.Build<ApprenticeshipDaysInLearning>()
                .With(l => l.LearnerId, _learner.Id)
                .With(l => l.NumberOfDaysInLearning, 365)
                .With(l => l.CollectionPeriodNumber, 2)
                .With(l => l.CollectionPeriodYear, 2223)
                .With(l => l.CreatedDate, new DateTime(2022, 10, 26))
                .Without(l => l.UpdatedDate)
                .Create();

            _apprenticeshipDaysInLearning3 = _fixture.Build<ApprenticeshipDaysInLearning>()
                .With(l => l.LearnerId, _learner.Id)
                .With(l => l.NumberOfDaysInLearning, 396)
                .With(l => l.CollectionPeriodNumber, 3)
                .With(l => l.CollectionPeriodYear, 2223)
                .With(l => l.CreatedDate, new DateTime(2022, 11, 16))
                .Without(l => l.UpdatedDate)
                .Create();

            _employmentCheck1 = _fixture.Build<EmploymentCheck>()
                .With(c => c.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(c => c.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                .With(c => c.MinimumDate, new DateTime(2021, 04, 01))
                .With(c => c.MaximumDate, new DateTime(2021, 09, 30))
                .With(c => c.Result, false)
                .With(c => c.CreatedDateTime, new DateTime(2022, 10, 01))
                .Without(c => c.ErrorType)
                .Create();

            _employmentCheck2 = _fixture.Build<EmploymentCheck>()
                .With(c => c.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(c => c.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                .With(c => c.MinimumDate, new DateTime(2021, 04, 01))
                .With(c => c.MaximumDate, new DateTime(2021, 11, 12))
                .With(c => c.Result, true)
                .With(c => c.CreatedDateTime, new DateTime(2022, 10, 01))
                .Without(c => c.ErrorType)
                .Create();

            _employmentCheck3 = _fixture.Build<EmploymentCheck>()
                .With(c => c.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(c => c.CheckType, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck)
                .With(c => c.MinimumDate, new DateTime(2022, 09, 30))
                .With(c => c.MaximumDate, new DateTime(2022, 10, 21))
                .With(c => c.Result, true)
                .With(c => c.CreatedDateTime, new DateTime(2022, 11, 01))
                .Without(c => c.ErrorType)
                .Create();

            _pendingPaymentValidationResult1 = _fixture.Build<PendingPaymentValidationResult>()
                .With(v => v.PendingPaymentId, _secondPendingPayment.Id)
                .With(v => v.Step, "EmployedAtStartOfApprenticeship")
                .With(v => v.Result, true)
                .With(v => v.PeriodNumber, 2)
                .With(v => v.PaymentYear, 2223)
                .With(v => v.OverrideResult, false)
                .Create();

            _pendingPaymentValidationResult2 = _fixture.Build<PendingPaymentValidationResult>()
                .With(v => v.PendingPaymentId, _secondPendingPayment.Id)
                .With(v => v.Step, "EmployedBeforeSchemeStarted")
                .With(v => v.Result, true)
                .With(v => v.PeriodNumber, 2)
                .With(v => v.PaymentYear, 2223)
                .With(v => v.OverrideResult, false)
                .Create();

            _pendingPaymentValidationResult3 = _fixture.Build<PendingPaymentValidationResult>()
                .With(v => v.PendingPaymentId, _secondPendingPayment.Id)
                .With(v => v.Step, "EmployedAt365Days")
                .With(v => v.Result, false)
                .With(v => v.PeriodNumber, 2)
                .With(v => v.PaymentYear, 2223)
                .With(v => v.OverrideResult, false)
                .Create();

            _payment1 = _fixture.Build<Payment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.PendingPaymentId, _firstPendingPayment.Id)
                .With(p => p.Amount, 1500)
                .With(p => p.CalculatedDate, new DateTime(2022, 11, 16))
                .With(p => p.PaidDate, new DateTime(2022, 11, 16))                
                .With(p => p.PaymentPeriod, (byte)1)
                .With(p => p.PaymentYear, (short)2223)
                .Create();          
        }

        private LearnerSubmissionDto CreateLearnerMatchApiData()
        {
            return _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.StartDate, new DateTime(2021, 10, 01))
                .With(s => s.IlrSubmissionDate, new DateTime(2022, 09, 26))
                .With(s => s.IlrSubmissionWindowPeriod, 3)
                .With(s => s.AcademicYear, "2223")
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){                               
                                _fixture
                                    .Build<PriceEpisodeDto>()
                                    .With(x => x.AcademicYear,"2223")
                                    .With(pe => pe.StartDate, new DateTime(2022, 10, 01))
                                    .With(pe => pe.EndDate, new DateTime(2023, 07, 31))
                                    .With(pe => pe.Periods, new List<PeriodDto>(){
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 3)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create()
                                    })
                                    .Create(),
                                _fixture
                                    .Build<PriceEpisodeDto>()
                                    .With(x => x.AcademicYear,"2122")
                                    .With(pe => pe.StartDate, new DateTime(2021, 10, 01))
                                    .With(pe => pe.EndDate, new DateTime(2022, 07, 31))
                                    .With(pe => pe.Periods, new List<PeriodDto>(){
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 3)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 4)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 5)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 6)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 7)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 8)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 9)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 10)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 11)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                        _fixture.Build<PeriodDto>()
                                            .With(period => period.Period, 12)
                                            .With(period => period.IsPayable, true)
                                            .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                            .Create(),
                                    })
                                    .Create()
                        }
                        )
                        .Create()}
                )
                .Create();
        }

        [Given(@"a learner has had a 365 plus 21 day first check")]
        public async Task GivenALearnerHasHadADayFirstCheck()
        {
            await _testContext.SetActiveCollectionCalendarPeriod(new CollectionPeriod() { Period = 3, Year = 2223 });

            await CreateIncentive();
        }


        [When(@"the stop date and resume is received in the same reporting period")]
        public Task WhenTheStopDateAndResumeIsReceivedInTheSameReportingPeriod()
        {
            _learnerMatchApiData = CreateLearnerMatchApiData();

            SetupMockLearnerMatchResponse(_learnerMatchApiData);
            return StartLearnerMatching();
        }

        [Then(@"the employment checks are re-inspired")]
        public void ThenTheEmploymentChecksAreReinspired()
        {
            // removal of the existing checks will initiate the new 365 checks when the checks are due
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            
            var employmentCheck = dbConnection
                .GetAll<EmploymentCheck>()
                .SingleOrDefault(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);

            employmentCheck.Should().BeNull();

            employmentCheck = dbConnection
                .GetAll<EmploymentCheck>()
                .Single(x => x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);

            employmentCheck.Result.Should().BeNull();

            employmentCheck = dbConnection
               .GetAll<EmploymentCheck>()
               .Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);

            employmentCheck.Result.Should().BeNull();
        }

        [Then(@"there is archiving of previous checks")]
        public void ThenThereIsArchivingOfPreviousChecks()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var employmentCheck = dbConnection
                .GetAll<Data.ApprenticeshipIncentives.Models.Archive.EmploymentCheck>()
                .Single(x => x.CheckType == "EmployedBeforeSchemeStarted");

            employmentCheck.EmploymentCheckId.Should().Be(_employmentCheck1.Id);

            employmentCheck = dbConnection
                .GetAll<Data.ApprenticeshipIncentives.Models.Archive.EmploymentCheck>()
                .Single(x => x.CheckType == "EmployedAtStartOfApprenticeship");

            employmentCheck.EmploymentCheckId.Should().Be(_employmentCheck2.Id);

            employmentCheck = dbConnection
                .GetAll<Data.ApprenticeshipIncentives.Models.Archive.EmploymentCheck>()
                .Single(x => x.CheckType == "EmployedAt365PaymentDueDateFirstCheck");

            employmentCheck.EmploymentCheckId.Should().Be(_employmentCheck3.Id);
        }

        private async Task CreateIncentive()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertWithEnumAsStringAsync(_firstPendingPayment);
            await dbConnection.InsertWithEnumAsStringAsync(_secondPendingPayment);
            await dbConnection.InsertAsync(_learner);
            await dbConnection.InsertAsync(_learningPeriod);
            await dbConnection.InsertAsync(_apprenticeshipDaysInLearning1);
            await dbConnection.InsertAsync(_apprenticeshipDaysInLearning2);
            await dbConnection.InsertAsync(_apprenticeshipDaysInLearning3);
            await dbConnection.InsertWithEnumAsStringAsync(_employmentCheck1);
            await dbConnection.InsertWithEnumAsStringAsync(_employmentCheck2);
            await dbConnection.InsertWithEnumAsStringAsync(_employmentCheck3);
            await dbConnection.InsertWithEnumAsStringAsync(_pendingPaymentValidationResult1);
            await dbConnection.InsertWithEnumAsStringAsync(_pendingPaymentValidationResult2);
            await dbConnection.InsertWithEnumAsStringAsync(_pendingPaymentValidationResult3);
            await dbConnection.InsertWithEnumAsStringAsync(_payment1);        
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
