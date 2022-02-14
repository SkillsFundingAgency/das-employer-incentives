using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using TechTalk.SpecFlow;
using ApprenticeshipIncentive = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ApprenticeshipIncentive;
using EmploymentCheck = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.EmploymentCheck;
using Learner = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Learner;
using Payment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Payment;
using PendingPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPayment;
using PendingPaymentValidationResult = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPaymentValidationResult;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ApplicationsForAccountRequested")]
    public class ApplicationsForAccountRequestedSteps : StepsBase
    {
        private Fixture _fixture;
        private GetApplicationsResponse _apiResponse;
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private Learner _learner;
        private List<PendingPayment> _pendingPayments;
        private Payment _payment;
        private CollectionCalendarPeriod _activePeriod;

        public ApplicationsForAccountRequestedSteps(TestContext testContext) : base(testContext)
        {
            _fixture = new Fixture();
        }

        [Given(@"an account that is in employer incentives")]
        public async Task GivenAnAccountThatIsInEmployerIncentives()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await SetupApprenticeshipIncentive();
        }

        [Given(@"an account that is in employer incentives with a signed employer agreement version of '(.*)'")]
        public async Task GivenAnAccountWithASignedEmployerAgreementVersion(string signedAgreementVersion)
        {
            var signedAgreementVersionValue = ParseNullableInt(signedAgreementVersion);
            _account = TestContext.TestData.GetOrCreate<Account>();
            _account.SignedAgreementVersion = signedAgreementVersionValue;
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await SetupApprenticeshipIncentive();
        }

        [When(@"there is no learner record for an apprenticeship")]
        public void WhenThereIsNoLearnerRecordForAnApprenticeship()
        {

        }

        [When(@"there is a learner record with no learner match for an apprenticeship")]
        public async Task WhenThereIsALearnerRecordWithNoLearnerMatch()
        {
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.LearningFound = false;
            await SetupLearner();
        }

        [When(@"there is a learner record with a learner match for an apprenticeship")]
        public async Task WhenThereIsALearnerRecordWithALearnerMatch()
        {
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.LearningFound = true;
            await SetupLearner();
        }

        [When(@"there is a learner record with a data lock for an apprenticeship")]
        public async Task WhenThereIsALearnerRecordWithADataLock()
        {
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.HasDataLock = true;
            await SetupLearner();
        }

        [When(@"there is a learner record with in learning set to false for an apprenticeship")]
        public async Task WhenThereIsALearnerRecordWithInLearningSetToFalse()
        {
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.InLearning = false;
            await SetupLearner();
        }

        [When(@"there is an incentive with payments paused for an apprenticeship")]
        public async Task WhenThereIsAnIncentiveRecordWithPausedPayments()
        {
            _apprenticeshipIncentive.PausePayments = true;
            await UpdateApprenticeshipIncentive();
        }

        [When(@"there is a '(.*)' payment that has been sent")]
        public async Task WhenThereIsAPaymentThatHasBeenSent(EarningType earningType)
        {
            _payment = _fixture.Build<Payment>()
                .With(x => x.AccountId, _account.Id)
                .With(x => x.AccountLegalEntityId, _account.AccountLegalEntityId)
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.PendingPaymentId, _apprenticeshipIncentive.PendingPayments.First(x => x.EarningType == earningType).Id)
                .With(x => x.PaidDate, _apprenticeshipIncentive.PendingPayments.First().DueDate.AddDays(1))
                .Create();

            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.InLearning = true;
            
            await SetupPayment();
            await SetupLearner();
        }

        [When(@"there is a '(.*)' payment that has been calculated but not sent")]
        public async Task WhenThereIsAPaymentThatHasBeenCalculatedButNotSent(EarningType earningType)
        {
            _payment = _fixture.Build<Payment>()
                .With(x => x.AccountId, _account.Id)
                .With(x => x.AccountLegalEntityId, _account.AccountLegalEntityId)
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.PendingPaymentId, _apprenticeshipIncentive.PendingPayments.First(x => x.EarningType == earningType).Id)
                .With(x => x.CalculatedDate, DateTime.Today)
                .Without(x => x.PaidDate)
                .Create();

            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.InLearning = true;

            await SetupPayment();
            await SetupLearner();
        }

        [When(@"there is a '(.*)' payment that has been calculated but not generated")]
        public async Task WhenThereIsAPaymentThatHasBeenCalculatedButNotGenerated(EarningType earningType)
        {
            var testPendingPayment = _pendingPayments.First(x => x.EarningType == earningType);
            testPendingPayment.DueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4);
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.InLearning = true;

            await UpdatePendingPayment(_pendingPayments.First(x => x.EarningType == earningType), testPendingPayment);
            await SetupLearner();
        }

        [When(@"there is a '(.*)' payment that has been calculated but not generated for the current period")]
        public async Task WhenThereIsAPaymentThatHasBeenCalculatedButNotGeneratedForTheCurrentPeriod(EarningType earningType)
        {
            _activePeriod = await GetActivePeriod();
            var testPendingPayment = _pendingPayments.First(x => x.EarningType == earningType);
            testPendingPayment.DueDate = _activePeriod.EIScheduledOpenDateUTC.Date.AddDays(-1);
            _learner = TestContext.TestData.GetOrCreate<Learner>();
            _learner.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
            _learner.InLearning = true;

            await UpdatePendingPayment(_pendingPayments.First(x => x.EarningType == earningType), testPendingPayment);
            await SetupLearner();
        }

        [When(@"there is an incentive with a minimum employer agreement version of '(.*)'")]
        public async Task WhenThereIsAnIncentiveWithAMinimumEmployerAgreementVersion(string minimumAgreementVersion)
        {
            var minimumAgreementVersionValue = ParseNullableInt(minimumAgreementVersion);
            _apprenticeshipIncentive.MinimumAgreementVersion = minimumAgreementVersionValue;
            await UpdateApprenticeshipIncentive();
        }

        [When(@"there is an '(.*)' payment validation status of '(.*)'")]
        public async Task WhenThereIsAnEmploymentCheckPaymentValidationStatus(string validationStep, bool validationResult)
        {
            var paymentValidationResult = _fixture.Build<PendingPaymentValidationResult>()
                .With(x => x.PendingPaymentId, _apprenticeshipIncentive.PendingPayments.First().Id)
                .With(x => x.Step, validationStep)
                .With(x => x.Result, validationResult)
                .Create();
            await SetupPaymentValidationResult(paymentValidationResult);
        }

        [When(@"there is an '(.*)' employment check result of '(.*)'")]
        public async Task WhenThereIsAnEmploymentCheckResult(EmploymentCheckType employmentCheckType, bool checkResult)
        {
            var employmentCheck = _fixture.Build<EmploymentCheck>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.CheckType, employmentCheckType)
                .With(x => x.Result, checkResult)
                .Create();
            await SetupEmploymentCheck(employmentCheck);
        }

        [When(@"a client requests the apprenticeships for the account")]
        public async Task WhenAClientRequestsTheApprenticeshipApplicationsForTheAccount()
        {
            var url = $"/accounts/{_account.Id}/legalentity/{_account.AccountLegalEntityId}/applications";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<GetApplicationsResponse>(url);

            status.Should().Be(HttpStatusCode.OK);

            _apiResponse = data;
        }

        [Then(@"the apprenticeships are returned")]
        public void ThenTheApprenticeshipApplicationsAreReturned()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var currentPeriodDate = new DateTime(TestContext.ActivePeriod.EIScheduledOpenDateUTC.Year, TestContext.ActivePeriod.EIScheduledOpenDateUTC.Month, TestContext.ActivePeriod.EIScheduledOpenDateUTC.Day);

            apprenticeshipApplication.AccountId.Should().Be(_account.Id);
            apprenticeshipApplication.FirstName.Should().Be(_apprenticeshipIncentive.FirstName);
            apprenticeshipApplication.LastName.Should().Be(_apprenticeshipIncentive.LastName);
            apprenticeshipApplication.LegalEntityName.Should().Be(_account.LegalEntityName);
            apprenticeshipApplication.TotalIncentiveAmount.Should().Be(_apprenticeshipIncentive.PendingPayments.Sum(x => x.Amount));
            apprenticeshipApplication.SubmittedByEmail.Should().Be(_apprenticeshipIncentive.SubmittedByEmail);
            // ReSharper disable once PossibleInvalidOperationException
            apprenticeshipApplication.ApplicationDate.Date.Should().Be(_apprenticeshipIncentive.SubmittedDate.Value.Date);

            apprenticeshipApplication.FirstPaymentStatus.Should().BeEquivalentTo(new
            {
                PaymentAmount = _apprenticeshipIncentive.PendingPayments.First().Amount,
                PaymentDate =  _apprenticeshipIncentive.PendingPayments.First().DueDate <= currentPeriodDate ? new DateTime(currentPeriodDate.AddMonths(1).Year, currentPeriodDate.AddMonths(1).Month, 27) : _apprenticeshipIncentive.PendingPayments.First().DueDate.AddMonths(1)
            });
            apprenticeshipApplication.SecondPaymentStatus.Should().BeEquivalentTo(new
            {
                PaymentAmount = _apprenticeshipIncentive.PendingPayments.Last().Amount,
                PaymentDate = _apprenticeshipIncentive.PendingPayments.Last().DueDate.AddMonths(1)
            });
        }

        [Then(@"the apprenticeship is returned with no learner match found")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithNoLearnerMatchFound()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.LearnerMatchFound.Should().BeFalse();
        }


        [Then(@"the apprenticeship is returned with learner match found")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithLearnerMatchFound()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.LearnerMatchFound.Should().BeTrue();
        }

        [Then(@"the apprenticeship is returned with data lock set to true")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithDataLockSetToTrue()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.HasDataLock.Should().BeTrue();
        }

        [Then(@"the apprenticeship is returned with in learning set to false")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithInLearningSetToFalse()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.InLearning.Should().BeFalse();
        }

        [Then(@"the apprenticeship is returned with payments paused set to true")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithPausePaymentsSetToTrue()
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.PausePayments.Should().BeTrue();
        }

        [Then(@"the apprenticeship is returned with the '(.*)' payment date set to the paid date")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithPaymentDateSetToPaidDate(EarningType earningType)
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);

            paymentStatus.PaymentSent.Should().BeTrue();
            paymentStatus.PaymentDate.Should().Be(_payment.PaidDate);
        }

        [Then(@"the '(.*)' payment amount is set to the calculated payment amount")]
        public void ThenThePaymentAmountIsSetToTheCalculatedPaymentAmount(EarningType earningType)
        {
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentAmount.Should().Be(_payment.Amount);
        }

        [Then(@"the apprenticeship is returned with the '(.*)' payment date set to the calculated date")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithPaymentDateSetToCalculatedDate(EarningType earningType)
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentSent.Should().BeFalse();
            paymentStatus.PaymentDate.Should().Be(_payment.CalculatedDate);
        }

        [Then(@"the '(.*)' payment amount is set to the pending payment amount")]
        public void ThenThePaymentAmountIsSetToThePendingPaymentAmount(EarningType earningType)
        {
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentAmount.Should().Be(_apprenticeshipIncentive.PendingPayments.First(x => x.EarningType == earningType).Amount);
        }

        [Then(@"the apprenticeship is returned with the '(.*)' estimated payment date set to the following month")]
        public void ThenTheApprenticeshipApplicationIsReturnedWithPaymentDateEstimatedToTheFollowingMonth(EarningType earningType)
        {
            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentSent.Should().BeFalse();
            paymentStatus.PaymentDate.Should().NotBeNull();
            paymentStatus.PaymentDate.Value.Day.Should().Be(4);
            paymentStatus.PaymentDate.Value.Month.Should().Be(_pendingPayments.First(x => x.EarningType == earningType).DueDate.AddMonths(1).Month);
            paymentStatus.PaymentDate.Value.Year.Should().Be(_pendingPayments.First(x => x.EarningType == earningType).DueDate.AddMonths(1).Year);
        }

        [Then(@"the apprenticeship is returned with the '(.*)' payment date set to the next active period")]
        public async Task ThenTheApprenticeshipApplicationIsReturnedWithPaymentDateSetToNextActivePeriod(EarningType earningType)
        {
            var nextPeriod = await GetPeriod(_activePeriod.Id + 1);

            _apiResponse.ApprenticeApplications.Should().NotBeEmpty();
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentSent.Should().BeFalse();
            paymentStatus.PaymentDate.Should().NotBeNull();
            paymentStatus.PaymentDate.Value.Day.Should().Be(27);
            paymentStatus.PaymentDate.Value.Month.Should().Be(nextPeriod.CalendarMonth);
            paymentStatus.PaymentDate.Value.Year.Should().Be(nextPeriod.CalendarYear);
        }

        [Then(@"the '(.*)' payment estimated is set to (.*)")]
        public void ThenThePaymentEstimatedIsSetTo(EarningType earningType, bool isEstimated)
        {
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            var paymentStatus = GetPaymentStatus(earningType, apprenticeshipApplication);
            paymentStatus.PaymentSentIsEstimated.Should().Be(isEstimated);
        }

        [Then(@"the apprenticeship is returned with requires new employment agreement set to '(.*)'")]
        public void ThenTheNewEmployerAgreementRequiredIsSet(bool newAgreementRequired)
        {
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.RequiresNewEmployerAgreement.Should().Be(newAgreementRequired);
            apprenticeshipApplication.SecondPaymentStatus.RequiresNewEmployerAgreement.Should().Be(newAgreementRequired);
        }

        [Then(@"the apprenticeship is returned with employment check status of '(.*)'")]
        public void ThenTheEmploymentCheckStatusIsSet(bool employmentCheckStatus)
        {
            var apprenticeshipApplication = _apiResponse.ApprenticeApplications.First();
            apprenticeshipApplication.FirstPaymentStatus.EmploymentCheckPassed.Should().Be(employmentCheckStatus);
            apprenticeshipApplication.SecondPaymentStatus.EmploymentCheckPassed.Should().Be(employmentCheckStatus);
        }

        private async Task SetupApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account, false);
            await dbConnection.InsertAsync(_apprenticeshipIncentive, false);
            _apprenticeshipIncentive.PendingPayments = _apprenticeshipIncentive.PendingPayments.Take(2).ToList();
            _apprenticeshipIncentive.PendingPayments.First().EarningType = EarningType.FirstPayment;
            _apprenticeshipIncentive.PendingPayments.First().DueDate = new DateTime(2020, 1, 12);
            _apprenticeshipIncentive.PendingPayments.Last().EarningType = EarningType.SecondPayment;
            _apprenticeshipIncentive.PendingPayments.Last().DueDate = new DateTime(2020, 9, 11);

            _pendingPayments = _apprenticeshipIncentive.PendingPayments.ToList();

            foreach (var pendingPayment in _pendingPayments)
            {
                pendingPayment.DueDate = pendingPayment.DueDate.Date;
                pendingPayment.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
                await dbConnection.InsertAsync(pendingPayment, true);
            }
        }

        private async Task UpdateApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.UpdateAsync(_apprenticeshipIncentive);
        }

        private async Task SetupLearner()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_learner, false);
        }
        private async Task UpdatePendingPayment(PendingPayment existingPendingPayment, PendingPayment newPendingPayment)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            // Need to delete and recreate the record because Dapper UpdateAsync extension doesn't respect enums as strings
            await dbConnection.DeleteAsync(existingPendingPayment);
            await dbConnection.InsertAsync(newPendingPayment, true);
        }

        private async Task SetupPayment()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_payment, true);
        }

        private async Task SetupPaymentValidationResult(PendingPaymentValidationResult paymentValidationResult)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(paymentValidationResult, true);
        }

        private async Task SetupEmploymentCheck(EmploymentCheck employmentCheck)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(employmentCheck, true);
        }

        private async Task<CollectionCalendarPeriod> GetActivePeriod()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var collectionCalendarPeriods = dbConnection.GetAll<CollectionCalendarPeriod>();
            return collectionCalendarPeriods.FirstOrDefault(x => x.Active);
        }

        private async Task<CollectionCalendarPeriod> GetPeriod(int id)
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var collectionCalendarPeriods = dbConnection.GetAll<CollectionCalendarPeriod>();
            return collectionCalendarPeriods.FirstOrDefault(x => x.Id == id);
        }

        private int? ParseNullableInt(string textValue)
        {
            if (String.IsNullOrWhiteSpace(textValue) || textValue.ToLower() == "null")
            {
                return null;
            }

            int numericValue;
            int.TryParse(textValue, out numericValue);
            return numericValue;
        }

        private static PaymentStatusDto GetPaymentStatus(EarningType earningType,
            ApprenticeApplicationDto apprenticeshipApplication)
        {
            var paymentStatus = apprenticeshipApplication.FirstPaymentStatus;
            if (earningType == EarningType.SecondPayment)
            {
                paymentStatus = apprenticeshipApplication.SecondPaymentStatus;
            }

            return paymentStatus;
        }


    }
}
