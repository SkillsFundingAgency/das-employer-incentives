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
        private readonly LearnerSubmissionDto _learnerMatchApiData;
        private readonly DateTime _plannedStartDate;
        private readonly DateTime _periodEndDate;

        public LearningStoppedSteps(TestContext testContext)
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
                .With(p => p.StartDate, new DateTime(2020, 11, 1))
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
                            .With(pe => pe.EndDate, _periodEndDate)
                            .Create() }
                        )
                        .Create()}
                )
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

        [Given(@"the learner data identifies the learner as not in learning anymore")]
        public void GivenTheIncentiveLearnerDataIdentifiesTheLearnerAsNotInLearningAnymore()
        {
            SetupMockLearnerMatchResponse();            
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

        [Then(@"the stopped change of circumstance is saved")]
        public void ThenTheStoppedChangeOfCircumstancesIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single(coc=> coc.ChangeType == ChangeOfCircumstanceType.LearningStopped);

            change.ChangeType.Should().Be(ChangeOfCircumstanceType.LearningStopped);
            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(false.ToString());
            change.NewValue.Should().Be(true.ToString());
            change.ChangedDate.Should().Be(_periodEndDate.AddDays(1));
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
