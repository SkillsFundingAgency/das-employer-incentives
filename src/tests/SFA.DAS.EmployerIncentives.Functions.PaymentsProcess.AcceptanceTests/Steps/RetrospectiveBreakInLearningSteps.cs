//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Threading.Tasks;
//using AutoFixture;
//using Dapper.Contrib.Extensions;
//using FluentAssertions;
//using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
//using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
//using SFA.DAS.EmployerIncentives.Data.Models;
//using SFA.DAS.EmployerIncentives.Enums;
//using TechTalk.SpecFlow;

//namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
//{
//    [Binding]
//    [Scope(Feature = "RetrospectiveBreakInLearning")]
//    public class RetrospectiveBreakInLearningSteps
//    {
//        private DateTime _initialStartDate;
//        private DateTime _initialEndDate;
//        private List<PendingPayment> _newEarnings;
//        private readonly TestContext _testContext;
//        private readonly Fixture _fixture;
//        private readonly Account _accountModel;
//        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
//        private readonly ApprenticeshipBreakInLearning _apprenticeshipBreakInLearning;
//        private LearnerSubmissionDto _resumedLearnerMatchApiData;

//        private readonly Payment _payment;
//        private readonly ClawbackPayment _clawbackPayment;
//        private readonly PendingPayment _pendingPayment;

//        private readonly DateTime _plannedStartDate;
//        private DateTime _stoppedDate;
//        private readonly int _breakInLearning;
//        private readonly byte _periodNumber;

//        protected RetrospectiveBreakInLearningSteps(TestContext context)
//        {
//            _testContext = context;
//            _fixture = new Fixture();

//            _plannedStartDate = new DateTime(2020, 11, 10);
//            _breakInLearning = 31;
//            _accountModel = _fixture.Create<Account>();
//            _periodNumber = 1;

//            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
//                .With(p => p.DateOfBirth, new DateTime(1995, 10, 15)) // under 25
//                .With(p => p.AccountId, _accountModel.Id)
//                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
//                .With(p => p.HasPossibleChangeOfCircumstances, false)
//                .With(p => p.StartDate, _plannedStartDate)
//                .With(p => p.SubmittedDate, _plannedStartDate.AddDays(-30))
//                .With(p => p.RefreshedLearnerForEarnings, true)
//                .With(p => p.PausePayments, false)
//                .With(p => p.Status, IncentiveStatus.Stopped)
//                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
//                .With(p => p.Phase, Phase.Phase1)
//                .Create();

//            _apprenticeshipBreakInLearning = _fixture
//                .Build<ApprenticeshipBreakInLearning>()
//                .With(b => b.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
//                .With(b => b.StartDate, new DateTime(2021, 02, 08))
//                .With(b => b.EndDate, (DateTime?)null)
//                .Create();

//            _pendingPayment = _fixture.Build<PendingPayment>()
//                .With(p => p.AccountId, _accountModel.Id)
//                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
//                .With(p => p.DueDate, new DateTime(2021, 02, 07))
//                .With(p => p.ClawedBack, false)
//                .With(p => p.EarningType, EarningType.FirstPayment)
//                .With(p => p.PaymentYear, (short)2021)
//                .With(p => p.PeriodNumber, (byte)7)
//                .With(p => p.Amount, 750)
//                .Without(p => p.PaymentMadeDate)
//                .Create();

//            _payment = _fixture.Build<Payment>()
//                    .With(d => d.PendingPaymentId, _pendingPayment.Id)
//                    .With(d => d.AccountId, _accountModel.Id)
//                    .With(d => d.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
//                    .With(d => d.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
//                    .With(d => d.PaymentPeriod, _pendingPayment.PeriodNumber)
//                    .With(d => d.PaymentYear, _pendingPayment.PaymentYear)
//                    .With(d => d.PaidDate, _pendingPayment.DueDate)
//                    .With(d => d.Amount, _pendingPayment.Amount)
//                    .Create();

//            _clawbackPayment = _fixture.Build<ClawbackPayment>()
//                    .With(d => d.PendingPaymentId, _pendingPayment.Id)
//                    .With(d => d.PaymentId, _payment.Id)
//                    .With(d => d.DateClawbackSent, _pendingPayment.DueDate.AddDays(5))
//                    .With(d => d.AccountId, _accountModel.Id)
//                    .With(d => d.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
//                    .With(d => d.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
//                    .With(d => d.CollectionPeriod, _pendingPayment.PeriodNumber)
//                    .With(d => d.CollectionPeriodYear, _pendingPayment.PaymentYear)
//                    .Create();
//        }

//        [Given(@"an existing apprenticeship incentive with learning starting on (.*) and ending on (.*)")]
//        public async Task GivenAnExistingApprenticeshipIncentiveWithLearningStartingIn_Oct(DateTime startDate, DateTime endDate)
//        {
//            _initialStartDate = startDate;
//            _initialEndDate = endDate;
//            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
//            await dbConnection.InsertAsync(_accountModel);
//            await dbConnection.InsertAsync(_apprenticeshipIncentive);
//        }

//        [Given(@"a payment of £1000 is not sent in Period R07 2021")]
//        public void GivenAPaymentIsNotSent()
//        {
//        }

//        [Given(@"Learner data is updated with a Break in Learning of 28 days before the first payment due date")]
//        public async Task GivenABreakInLearningBeforeTheFirstPayment()
//        {
//            await SetupBreakInLearning("2021-02-27T00:00:00", "2021-03-28T00:00:00");
//        }

//        [Given(@"Learner data is updated with a Break in Learning of less than 28 days before the first payment due date")]
//        public async Task GivenAShortBreakInLearningBeforeTheFirstPayment()
//        {
//            await SetupBreakInLearning("2021-02-27T00:00:00", "2021-03-26T00:00:00");
//        }

//        private async Task SetupBreakInLearning(string breakStart, string breakEnd)
//        {
//            var priceEpisode1 = new PriceEpisodeDtoBuilder()
//                .WithAcademicYear(2021)
//                .WithStartDate(_initialStartDate)
//                .WithEndDate(breakStart)
//                .WithPeriod(TestData.ApprenticeshipId, 7)
//                .Create();

//            var priceEpisode2 = new PriceEpisodeDtoBuilder()
//                .WithAcademicYear(2021)
//                .WithStartDate(breakEnd)
//                .WithEndDate("2021-07-31T00:00:00")
//                .WithPeriod(TestData.ApprenticeshipId, 8)
//                .Create();

//            var learnerSubmissionData = new LearnerSubmissionDtoBuilder()
//                .WithUkprn(TestData.UKPRN)
//                .WithUln(TestData.ULN)
//                .WithAcademicYear(2021)
//                .WithIlrSubmissionDate("2021-02-11T14:06:18.673+00:00")
//                .WithIlrSubmissionWindowPeriod(8)
//                .WithStartDate(_initialStartDate)
//                .WithPriceEpisode(priceEpisode1)
//                .WithPriceEpisode(priceEpisode2)
//                .Create();

//            await Helper.LearnerMatchApiHelper.SetupResponse(TestData.ULN, TestData.UKPRN, learnerSubmissionData);
//        }

//        [When(@"the Learner Match is run in Period R(.*) (.*)")]
//        public async Task WhenTheLearnerMatchIsRunInPeriodR(byte period, short year)
//        {
//            await Helper.CollectionCalendarHelper.SetActiveCollectionPeriod(period, year);
//            await Helper.LearnerMatchOrchestratorHelper.Run();
//        }

//        [When(@"the earnings are recalculated")]
//        public void WhenTheEarningsAreRecalculated()
//        {
//            _newEarnings = Helper.EISqlHelper.GetAllFromDatabase<PendingPayment>()
//                .Where(x => x.ApprenticeshipIncentiveId == TestData.ApprenticeshipIncentiveId).ToList();
//        }

//        [Then(@"the Break in Learning is recorded")]
//        public async Task ThenTheBreakInLearningIsRecorded()
//        {
//            var breaksInLearning = Helper.EISqlHelper.GetAllFromDatabase<ApprenticeshipBreakInLearning>()
//                .Where(x => x.ApprenticeshipIncentiveId == TestData.ApprenticeshipIncentiveId).ToList();

//            breaksInLearning.Count.Should().Be(1);
//            breaksInLearning.Single().StartDate.Should().Be(new DateTime(2021, 02, 28));
//            breaksInLearning.Single().EndDate.Should().Be(new DateTime(2021, 03, 28));
//        }

//        [Then(@"no Break in Learning is recorded")]
//        public async Task ThenNoBreakInLearningIsRecorded()
//        {
//            var breaksInLearning = Helper.EISqlHelper.GetAllFromDatabase<ApprenticeshipBreakInLearning>()
//                .Where(x => x.ApprenticeshipIncentiveId == TestData.ApprenticeshipIncentiveId).ToList();

//            breaksInLearning.Count.Should().Be(0);
//        }

//        [Then(@"a new first pending payment of £(.*) is created for Period R(.*) (.*)")]
//        public void ThenANewFirstPendingPaymentOfIsCreated(int amount, byte period, short year)
//        {
//            AssertPendingPayment(amount, period, year, EarningType.FirstPayment);
//        }

//        [Then(@"a new second pending payment of £(.*) is created for Period R(.*) (.*)")]
//        public void ThenANewSecondPendingPaymentOfIsCreated(int amount, byte period, short year)
//        {
//            AssertPendingPayment(amount, period, year, EarningType.SecondPayment);
//        }

//        [Then(@"the pending payments are not changed")]
//        public void ThenThePendingPaymentsAreNotChanged()
//        {
//            AssertPendingPayment(1000, 7, 2021, EarningType.FirstPayment);
//            AssertPendingPayment(1000, 4, 2122, EarningType.SecondPayment);
//        }

//        [Then(@"the Learner is In Learning")]
//        public void ThenTheLearnerIsInLearning()
//        {
//            var learner = Helper.EISqlHelper.GetFromDatabase<Learner>(x => x.ApprenticeshipIncentiveId == TestData.ApprenticeshipIncentiveId);
//            learner.InLearning.Should().BeTrue();
//        }

//        private void AssertPendingPayment(int amount, byte period, short year, EarningType earningType)
//        {
//            var pp = _newEarnings.Single(x =>
//                x.ApprenticeshipIncentiveId == TestData.ApprenticeshipIncentiveId
//                && x.EarningType == earningType
//                && !x.ClawedBack);

//            pp.Amount.Should().Be(amount);
//            pp.PaymentMadeDate.Should().BeNull();
//            pp.PeriodNumber.Should().Be(period);
//            pp.PaymentYear.Should().Be(year);
//        }
//    }
//}
