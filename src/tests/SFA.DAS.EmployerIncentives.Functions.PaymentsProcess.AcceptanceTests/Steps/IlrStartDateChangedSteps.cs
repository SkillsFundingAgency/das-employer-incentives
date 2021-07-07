using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IlrStartDateChanged")]
    public partial class IlrStartDateChangedSteps
    {
        [Given(@"a '(.*)' apprenticeship incentive exists")]
        public async Task GivenAnApprenticeshipIncentiveExists(string phase)
        {
            CreateIncentive(phase);
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
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PaidDate, DateTime.Now.AddDays(-1))
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.Amount, _pendingPayment.Amount)
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
            _actualStartDate = new DateTime(2021, 5, 31);
        }

        [When(@"the learner data is updated with new invalid start date for the apprenticeship incentive")]
        public void WhenTheLearnerDataIsUpdatedWithNewInvalidStartDateForTheApprenticeshipIncentive()
        {
            _actualStartDate = _plannedStartDate.AddYears(-3);
        }

        [When(@"the incentive learner data is refreshed")]
        public async Task WhenTheIncentiveLearnerDataIsRefreshed()
        {
            _learnerMatchApiData.Training.First().PriceEpisodes.First().StartDate = _actualStartDate;
            _learnerMatchApiData.Training.First().PriceEpisodes.First().EndDate = _actualStartDate.AddYears(3);
            SetupMockLearnerMatchResponse();
            await StartLearnerMatching();
        }

        [When(@"the learner data is updated with a new valid start date for the apprenticeship incentive making the learner over twenty five at start")]
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

        [Then(@"the start date change of circumstance is saved")]
        public void ThenTheStartDateChangeOfCircumstanceIsSaved()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var change = dbConnection.GetAll<ChangeOfCircumstance>().Single();

            change.ChangeType.Should().Be(ChangeOfCircumstanceType.StartDate);
            change.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            change.PreviousValue.Should().Be(_plannedStartDate.ToString("yyy-MM-dd"));
            change.NewValue.Should().Be(_actualStartDate.ToString("yyy-MM-dd"));
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

            var clawback = dbConnection.GetAll<ClawbackPayment>().Single(p => p.PendingPaymentId == _pendingPayment.Id);
            clawback.Should().BeEquivalentTo(_payment, opt => opt.ExcludingMissingMembers()
                .Excluding(x => x.Amount)
                .Excluding(x => x.Id));
            clawback.Amount.Should().Be(-1000);
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
            dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.PendingPayment>().Count(p =>
                p.ApprenticeshipIncentiveId == _pendingPayment.ApprenticeshipIncentiveId)
                .Should().Be(1);
            var archivedPendingPayment = dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.PendingPayment>().Single(p =>
                p.PendingPaymentId == _pendingPayment.Id);
            archivedPendingPayment.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
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
            dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.Payment>()
                .Count(p => p.ApprenticeshipIncentiveId == _payment.ApprenticeshipIncentiveId)
                .Should().Be(1);
            var archivedPayment = dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.Payment>().Single(p => p.PaymentId == _payment.Id);
            archivedPayment.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            archivedPayment.Should().BeEquivalentTo(_payment, opt => opt.ExcludingMissingMembers()
                    .Excluding(x => x.CalculatedDate) // millisecond difference due to SQL DateTime2 to .NET DateTime conversion
                );
            archivedPayment.CalculatedDate.Should().BeCloseTo(archivedPayment.CalculatedDate, TimeSpan.FromSeconds(1));
        }

        [Then(@"all pending payment validation results are archived")]
        public void ThenAllPendingPaymentValidationResultsAreArchived()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var archivedValidationResult = dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.PendingPaymentValidationResult>().Single(p =>
               p.PendingPaymentId == _pendingPayment.Id);
            archivedValidationResult.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            archivedValidationResult.Should().BeEquivalentTo(_pendingPaymentValidationResult, opt => opt.ExcludingMissingMembers()
                .Excluding(x => x.CreatedDateUtc));
            archivedValidationResult.CreatedDateUtc.Should().BeCloseTo(_pendingPaymentValidationResult.CreatedDateUtc, TimeSpan.FromMinutes(1));
        }

        [Then(@"earnings are recalculated")]
        public void ThenEarningsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _newPendingPayments = dbConnection.GetAll<PendingPayment>().ToList();
        }

        [Then(@"a new first earning of '(.*)' is created")]
        public async Task ThenANewFirstEarningOfIsCreated(decimal amount)
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.FirstPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(amount);
            pp.PaymentMadeDate.Should().BeNull();

            var period = await _testContext.GetCollectionCalendarPeriod(_actualStartDate.AddDays(90));
            pp.PeriodNumber.Should().Be(period.PeriodNumber);
            pp.PaymentYear.ToString().Should().Be(period.AcademicYear);
        }

        [Then(@"a new second earning of '(.*)' is created")]
        public async Task ThenANewSecondEarningOfIsCreated(decimal amount)
        {
            var pp = _newPendingPayments.Single(x =>
                x.AccountId == _accountModel.Id
                && x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.AccountLegalEntityId == _accountModel.AccountLegalEntityId
                && x.EarningType == EarningType.SecondPayment
                && !x.ClawedBack);

            pp.Amount.Should().Be(amount);
            pp.PaymentMadeDate.Should().BeNull();
            var period = await _testContext.GetCollectionCalendarPeriod(_actualStartDate.AddDays(365));
            pp.PeriodNumber.Should().Be(period.PeriodNumber);
            pp.PaymentYear.ToString().Should().Be(period.AcademicYear);
        }

        [Then(@"existing payment record is retained")]
        public void ThenExistingPaymentRecordIsRetained()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.Payment>().SingleOrDefault(x => x.PendingPaymentId == _pendingPayment.Id)
                .Should().BeNull("Should not have archived existing payment");

            dbConnection.GetAll<Payment>().SingleOrDefault(x => x.PendingPaymentId == _pendingPayment.Id)
                .Should().NotBeNull("Should not have deleted existing payment");
        }

        [Then(@"existing pending payment validation record is retained")]
        public void ThenExistingPendingPaymentValidationRecordIsRetained()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            dbConnection.GetAll<Data.ApprenticeshipIncentives.Models.Archive.PendingPaymentValidationResult>().SingleOrDefault(x => x.PendingPaymentId == _pendingPayment.Id)
                .Should().BeNull("Should not have archived existing pending payment validation result");

            dbConnection.GetAll<PendingPaymentValidationResult>().SingleOrDefault(x => x.PendingPaymentId == _pendingPayment.Id)
                .Should().NotBeNull("Should not have deleted existing pending payment validation result");
        }

        [Then(@"the minimum agreement version is changed to '(.*)'")]
        public void ThenTheMinimumAgreementVersionIsChanged(short version)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var incentive = dbConnection.GetAll<ApprenticeshipIncentive>()
                .Single(x => x.Id == _apprenticeshipIncentive.Id);

            incentive.MinimumAgreementVersion.Should().Be(version);
        }
        
    }
}
