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
    [Scope(Feature = "IlrStartDateChanged")]
    public class IlrStartDateChangedSteps
    {
        private readonly TestContext _testContext;
        private readonly Account _accountModel;
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly LearnerSubmissionDto _learnerMatchApiData;
        private readonly DateTime _plannedStartDate;
        private PendingPaymentValidationResult _pendingPaymentValidationResult;
        private Payment _payment;
        private List<PendingPayment> _newPendingPayments;
        private DateTime _actualStartDate;

        public IlrStartDateChangedSteps(TestContext testContext)
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
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, _plannedStartDate.AddMonths(1))
                .With(p => p.PeriodNumber, (byte?)1)
                .With(p => p.PaymentYear, (short?)2021)
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _pendingPaymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.PeriodNumber, _pendingPayment.PeriodNumber)
                .With(p => p.PaymentYear, _pendingPayment.PaymentYear)
                .With(p => p.Step, "HasBankDetails")
                .With(p => p.Result, true)
                .Create();

            _learnerMatchApiData = _fixture
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
                            .With(pe => pe.EndDate, _plannedStartDate.AddYears(1))
                            .Create() }
                        )
                        .Create()}
                )
                .Create();
        }

        [Given(@"an apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_pendingPaymentValidationResult);
        }

        [Given(@"an earning has been paid for an apprenticeship incentive application")]
        public async Task WhenTheExistingEarningHasBeenPaid()
        {
            _payment = _fixture.Build<Payment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.PaidDate, DateTime.Now.AddDays(-1))
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.PaymentYear, _pendingPayment.PaymentYear)
                .With(p => p.PaymentPeriod, _pendingPayment.PeriodNumber)
                .Create();

            _pendingPayment.PaymentMadeDate = DateTime.Now.AddDays(-1);

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_payment);
            await dbConnection.UpdateAsync(_pendingPayment);
        }

        [When(@"the learner data is updated with new valid start date for the apprenticeship incentive")]
        public void WhenTheLearnerDataIsUpdatedWithNewValidStartDateForTheApprenticeshipIncentive()
        {
            _actualStartDate = _plannedStartDate.AddMonths(1);
        }

        [When(@"the learner data is updated with new invalid start date for the apprenticeship incentive")]
        public void WhenTheLearnerDataIsUpdatedWithNewInvalidStartDateForTheApprenticeshipIncentive()
        {
            _actualStartDate = _plannedStartDate.AddMonths(-1);
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            _learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate = _actualStartDate;
            _learnerMatchApiData.Training.First().PriceEpisodes.First().EndDate = _actualStartDate.AddYears(1);
            SetupMockLearnerMatchResponse();
            await StartLearnerMatching();
        }

        [When(@"the learner data is refreshed with a new valid start date for the apprenticeship incentive making the learner over twenty five at start")]
        public void WhenTheLearnerDataIsRefreshedWithANewValidStartDateForTheApprenticeshipIncentiveMakingTheLearnerOverTwentyFiveAtStart()
        {
            _actualStartDate = _plannedStartDate.AddMonths(2);
        }

        [Then(@"the actual start date is updated")]
        public void ThenActualStartDateIsUpdated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>();

            incentive.Single().StartDate.Should().Be(_learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate);
        }

        [Then(@"the pending payments are recalculated for the apprenticeship incentive")]
        public void ThenPendingPaymentsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Count().Should().Be(2);
        }

        [Then(@"the learner data is subsequently refreshed")]
        public void ThenLearnerRefreshIsCalledAgain()
        {
            _testContext.LearnerMatchApi.MockServer.LogEntries.Count(x =>
                    x.RequestMessage.Path == $"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                .Should().Be(2);
        }

        [Then(@"the existing pending payments are removed")]
        public void ThenPendingPaymentsAreRemoved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();

            pendingPayments.Should().BeEmpty();
        }

        [Then(@"the paid earning is marked as requiring a clawback")]
        public void ThenThePaidEarningIsMarkedAsRequiringAClawback()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var pendingPayment = dbConnection.GetAll<PendingPayment>().Single(p => p.Id == _pendingPayment.Id);
            pendingPayment.ClawedBack.Should().BeTrue();
        }

        [Given(@"an earning has not been paid for an apprenticeship incentive application")]
        public async Task GivenAnEarningHasNotBeenPaidForAnApprenticeshipIncentiveApplication()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            _payment = _fixture.Build<Payment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .Without(p => p.PaidDate)
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.PaymentYear, _pendingPayment.PaymentYear)
                .With(p => p.PaymentPeriod, _pendingPayment.PeriodNumber)
                .Create();
            await dbConnection.InsertAsync(_payment);
        }

        [Then(@"the unpaid earning is archived")]
        public void ThenTheUnpaidEarningIsArchived()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            dbConnection.GetAll<PendingPayment>().Any(p => p.PeriodNumber == _pendingPayment.PeriodNumber
                && p.PaymentYear == _pendingPayment.PaymentYear).Should()
                .BeFalse();
            var archivedPendingPayment = dbConnection.GetAll<ArchivedPendingPayment>().Single(p =>
                p.PendingPaymentId == _pendingPayment.Id);
            archivedPendingPayment.ArchivedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            archivedPendingPayment.Should().BeEquivalentTo(_pendingPayment, opt => opt.ExcludingMissingMembers()
                    .Excluding(x => x.CalculatedDate) // millisecond difference due to SQL DateTime2 to .NET DateTime conversion
            );
            archivedPendingPayment.CalculatedDate.Should().BeCloseTo(_pendingPayment.CalculatedDate, TimeSpan.FromSeconds(1));
        }

        [Then(@"all unpaid payment records are archived")]
        public void ThenAllUnpaidPaymentRecordsAreArchived()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            dbConnection.GetAll<Payment>().Any(p =>
                    p.PaymentPeriod == _pendingPayment.PeriodNumber && p.PaymentYear == _pendingPayment.PaymentYear)
                .Should().BeFalse();
            var archivedPayment = dbConnection.GetAll<ArchivedPayment>().Single(p => p.PaymentId == _payment.Id);
            archivedPayment.ArchivedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            archivedPayment.Should().BeEquivalentTo(_payment, opt => opt.ExcludingMissingMembers()
                    .Excluding(x => x.CalculatedDate) // millisecond difference due to SQL DateTime2 to .NET DateTime conversion
                );
            archivedPayment.CalculatedDate.Should().BeCloseTo(archivedPayment.CalculatedDate, TimeSpan.FromSeconds(1));
        }

        [Then(@"all pending payment validation results are archived")]
        public void ThenAllPendingPaymentValidationResultsAreArchived()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var archivedValidationResult = dbConnection.GetAll<ArchivedPendingPaymentValidationResult>().Single(p =>
                p.PendingPaymentId == _pendingPayment.Id);
            archivedValidationResult.ArchivedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            archivedValidationResult.Should().BeEquivalentTo(_pendingPaymentValidationResult, opt => opt.ExcludingMissingMembers()
                .Excluding(x => x.CreatedDateUtc));
            archivedValidationResult.CreatedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Then(@"earnings are recalculated")]
        public void ThenEarningsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _newPendingPayments = dbConnection.GetAll<PendingPayment>().ToList();
        }

        [Then(@"a new pending first payment record is created")]
        public void ThenANewPendingFirstPaymentRecordIsCreated()
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.FirstPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(1000);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(4);
            pp.PaymentYear.Should().Be(2021);
        }

        [Then(@"a new pending second payment record is created")]
        public void ThenANewPendingSecondPaymentRecordIsCreated()
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.SecondPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(1000);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(1);
            pp.PaymentYear.Should().Be(2122);
        }

        [Then(@"a new pending first payment record is created with a new amount and payment period")]
        public void ThenANewPendingFirstPaymentRecordIsCreatedWithANewAmount()
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.FirstPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(750);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(5);
            pp.PaymentYear.Should().Be(2021);
        }

        [Then(@"a new pending second payment record is created with a new amount and payment period")]
        public void ThenANewPendingSecondPaymentRecordIsCreatedWithANewAmount()
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.SecondPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(750);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(2);
            pp.PaymentYear.Should().Be(2122);
        }

        private void SetupMockLearnerMatchResponse()
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
                    .WithBodyAsJson(_learnerMatchApiData));
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
