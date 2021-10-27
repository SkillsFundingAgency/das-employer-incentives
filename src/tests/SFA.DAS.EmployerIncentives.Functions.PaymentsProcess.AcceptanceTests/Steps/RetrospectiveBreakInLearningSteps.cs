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
    [Scope(Feature = "RetrospectiveBreakInLearning")]
    public class RetrospectiveBreakInLearningSteps
    {
        private DateTime _initialStartDate;
        private List<PendingPayment> _newEarnings;
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly Account _accountModel;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private LearnerSubmissionDto _resumedLearnerMatchApiData;
        private readonly PendingPayment _pendingPayment;

        private DateTime _breakStart;
        private DateTime _breakEnd;

        protected RetrospectiveBreakInLearningSteps(TestContext context)
        {
            _testContext = context;
            _fixture = new Fixture();
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, DateTime.Now.AddYears(-24)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.PausePayments, false)
                .With(p => p.Status, IncentiveStatus.Stopped)
                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(p => p.Phase, Phase.Phase1)
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.DueDate, new DateTime(2021, 02, 27))
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .With(p => p.PaymentYear, (short)2021)
                .With(p => p.PeriodNumber, (byte)7)
                .With(p => p.Amount, 1000)
                .Without(p => p.PaymentMadeDate)
                .Create();
        }

        [Given(@"an existing apprenticeship incentive with learning starting on (.*) and ending on (.*)")]
        public async Task GivenAnExistingApprenticeshipIncentiveWithLearningStartingIn_Oct(DateTime startDate, DateTime endDate)
        {
            _initialStartDate = startDate;
            _apprenticeshipIncentive.StartDate = startDate;

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
        }

        [Given(@"a payment of £1000 is not sent in Period R07 2021")]
        public void GivenAPaymentIsNotSent()
        {
            // blank
        }

        [Given(@"Learner data is updated with a Break in Learning of 28 days before the first payment due date")]
        public void GivenABreakInLearningBeforeTheFirstPayment()
        {
            SetupBreakInLearning(DateTime.Parse("2021-02-25T00:00:00"), DateTime.Parse("2021-03-25T00:00:00"));
        }

        [Given(@"Learner data is updated with a Break in Learning of less than 28 days before the first payment due date")]
        public void GivenAShortBreakInLearningBeforeTheFirstPayment()
        {
            SetupBreakInLearning(DateTime.Parse("2021-02-28T00:00:00"), DateTime.Parse("2021-03-27T00:00:00"));
        }

        private void SetupBreakInLearning(DateTime start, DateTime end)
        {
            _breakStart = start;
            _breakEnd = end;

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
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.StartDate, _initialStartDate)
                                        .With(pe => pe.EndDate, _breakStart.AddDays(-1))
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, 7)
                                                .Create()
                                        })
                                        .Create(),
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.StartDate, _breakEnd)
                                        .With(pe => pe.EndDate, new DateTime(2021, 7, 31))
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, 8)
                                                .Create()
                                        })

                                        .Create()
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            SetupMockLearnerMatchResponse(_resumedLearnerMatchApiData);
        }

        [When(@"the Learner Match is run in Period R(.*) (.*)")]
        public async Task WhenTheLearnerMatchIsRunInPeriodR(byte period, short year)
        {
            await _testContext.SetActiveCollectionCalendarPeriod(year, period);
            await StartLearnerMatching();
        }

        [When(@"the earnings are recalculated")]
        public void WhenTheEarningsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _newEarnings = dbConnection.GetAll<PendingPayment>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();
        }

        [Then(@"the Break in Learning is recorded")]
        public async Task ThenTheBreakInLearningIsRecorded()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count.Should().Be(1);
            breaksInLearning.Single().StartDate.Should().Be(_breakStart);
            breaksInLearning.Single().EndDate.Should().Be(_breakEnd.AddDays(-1));
        }

        [Then(@"no Break in Learning is recorded")]
        public async Task ThenNoBreakInLearningIsRecorded()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count.Should().Be(0);
        }

        [Then(@"a new first pending payment of £(.*) is created for Period R(.*) (.*)")]
        public void ThenANewFirstPendingPaymentOfIsCreated(int amount, byte period, short year)
        {
            AssertPendingPayment(amount, period, year, EarningType.FirstPayment);
        }

        [Then(@"a new second pending payment of £(.*) is created for Period R(.*) (.*)")]
        public void ThenANewSecondPendingPaymentOfIsCreated(int amount, byte period, short year)
        {
            AssertPendingPayment(amount, period, year, EarningType.SecondPayment);
        }

        [Then(@"the pending payments are not changed")]
        public void ThenThePendingPaymentsAreNotChanged()
        {
            AssertPendingPayment(1000, 7, 2021, EarningType.FirstPayment);
            AssertPendingPayment(1000, 4, 2122, EarningType.SecondPayment);
        }

        [Then(@"the Learner is In Learning")]
        public async Task ThenTheLearnerIsInLearning()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>()
                .Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);
            
            learner.InLearning.Should().BeTrue();
        }

        private void AssertPendingPayment(int amount, byte period, short year, EarningType earningType)
        {
            var pp = _newEarnings.Single(x =>
                x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.EarningType == earningType
                && !x.ClawedBack);

            pp.Amount.Should().Be(amount);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(period);
            pp.PaymentYear.Should().Be(year);
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
