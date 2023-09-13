using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using TechTalk.SpecFlow;
using Payment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Payment;
using PendingPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPayment;
using PendingPaymentValidationResult = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPaymentValidationResult;
using ValidationOverride = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ValidationOverride;
using System;
using AutoFixture;
using SFA.DAS.EmployerIncentives.Enums;
using EmploymentCheck = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ValidatePayments")]
    public partial class ValidatePaymentsSteps
    {
        private readonly TestContext _testContext;
        protected static short CollectionPeriodYear = 2021;
        protected static byte CollectionPeriod = 6;
        private Fixture _fixture;

        private ValidatePaymentData _validatePaymentData;

        public ValidatePaymentsSteps(TestContext testContext)
        {
            _fixture = new Fixture();
            _testContext = testContext;
            _validatePaymentData = new ValidatePaymentData(_testContext);
        }

        [Given(@"there are pending payments")]
        public void GivenThereArePendingPayments()
        {
            //
        }

        [Given(@"no validations steps will fail")]
        public void GivenNoValidationsStepsWillFailed()
        {
            //
        }

        [Given(@"the '(.*)' will fail")]
        public void GivenTheValidationStepWillFail(string validationStep)
        {
            switch (validationStep)
            {
                case ValidationStep.HasBankDetails:
                    _validatePaymentData.AccountModel.VrfVendorId = null; // no bank details
                    break;
                case ValidationStep.IsInLearning:
                    _validatePaymentData.LearnerModel.InLearning = false;
                    break;
                case ValidationStep.HasLearningRecord:
                    _validatePaymentData.LearnerModel.LearningFound = false;
                    break;
                case ValidationStep.HasNoDataLocks:
                    _validatePaymentData.LearnerModel.HasDataLock = true;
                    break;
                case ValidationStep.HasIlrSubmission:
                    _validatePaymentData.LearnerModel.SubmissionFound = false;
                    _validatePaymentData.LearnerModel.SubmissionDate = null;
                    break;
                case ValidationStep.HasDaysInLearning:
                    _validatePaymentData.DaysInLearning.NumberOfDaysInLearning = 89;
                    break;
                case ValidationStep.PaymentsNotPaused:
                    _validatePaymentData.ApprenticeshipIncentiveModel.PausePayments = true;
                    break;
                case ValidationStep.HasSignedMinVersion:
                    _validatePaymentData.ApprenticeshipIncentiveModel.MinimumAgreementVersion = _validatePaymentData.AccountModel.SignedAgreementVersion + 1;
                    break;
                case ValidationStep.LearnerMatchSuccessful:
                    _validatePaymentData.LearnerModel.SuccessfulLearnerMatchExecution = false;
                    break;
                case ValidationStep.EmployedAtStartOfApprenticeship:
                    _validatePaymentData.EmploymentChecks
                        .Single(c => c.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id &&
                                     c.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship)
                        .Result = false;
                    break;
                case ValidationStep.EmployedBeforeSchemeStarted:
                    _validatePaymentData.EmploymentChecks
                        .Single(c => c.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id &&
                                     c.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted)
                        .Result = true;                   
                    break;
                case ValidationStep.BlockedForPayments:
                    _validatePaymentData.AccountModel.VendorBlockEndDate = DateTime.Now.AddDays(1);
                    break;
            }
        }

        [Given(@"an apprentice has a pending 365 day payment")]
        public void GivenAnApprenticeHasAPending365DayPayment()
        {
            _validatePaymentData.PendingPaymentModel1.PaymentMadeDate = _validatePaymentData.ApprenticeshipIncentiveModel.StartDate.AddDays(100);
            _validatePaymentData.PendingPaymentModel2.EarningType = EarningType.SecondPayment;
            _validatePaymentData.PendingPaymentModel2.DueDate = DateTime.Today.AddDays(-20);
            _validatePaymentData.PendingPaymentModel2.PaymentMadeDate = null;
            _validatePaymentData.DaysInLearning.NumberOfDaysInLearning = 365;
        }

        [Given(@"the employed at 365 days check will fail")]
        public void GivenTheEmployedAt365DaysCheckWillFail()
        {
            _validatePaymentData.EmploymentChecks.Single(x =>
                x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id
                && x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck).Result = false;
        }

        [Given(@"the employed at 365 days check has not been done")]
        public void GivenTheEmployedAt365DaysCheckHasNotBeenDone()
        {
            var check = _validatePaymentData.EmploymentChecks.SingleOrDefault(c => c.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);
            if(check != null)
            {
                _validatePaymentData.EmploymentChecks.Remove(check);
            }
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await _validatePaymentData.Create();

            await _testContext.SetActiveCollectionCalendarPeriod(new CollectionPeriod() { Period = CollectionPeriod, Year = CollectionPeriodYear });

            await _testContext.TestFunction.Start(
               new OrchestrationStarterInfo(
                   "IncentivePaymentOrchestrator_HttpStart",
                   nameof(IncentivePaymentOrchestrator),
                   new Dictionary<string, object>
                   {
                       ["req"] = TestContext.TestRequest($"/api/orchestrators/IncentivePaymentOrchestrator")
                   },
                   expectedCustomStatus: "WaitingForPaymentApproval"
                   ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var response = await _testContext.TestFunction.GetOrchestratorStartResponse();
            var status = await _testContext.TestFunction.GetStatus(response.Id);
            status.CustomStatus.ToObject<string>().Should().Be("WaitingForPaymentApproval");
        }

        [Then(@"the '(.*)' will have a failed validation result")]
        public async Task ThenTheValidationStepWillHaveAFailedValidationResult(string step)
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == step).ToList();
            results.Should().HaveCount(1);
            results.All(r => !r.Result).Should().BeTrue($"{step} validation step should have failed");
            results.All(r => r.CreatedDateUtc == DateTime.Today).Should().BeTrue();
        }

        [Then(@"the '(.*)' will have two failed validation results")]
        public async Task ThenTheValidationStepWillHaveTwoFailedValidationResult(string step)
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == step).ToList();
            results.Should().HaveCount(2);
            results.All(r => !r.Result).Should().BeTrue($"{step} validation step should have failed");
            results.All(r => r.CreatedDateUtc == DateTime.Today).Should().BeTrue();
        }

        [Then(@"successful validation results are recorded")]
        public async Task ThenSuccessValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = await connection.GetAllAsync<PendingPaymentValidationResult>();
            results.Should().NotBeEmpty();
            results.All(r => r.Result).Should().BeTrue();
        }

        [Then(@"the validation result override for '(.*)' is recorded")]
        public async Task ThenTheValidationResultOverrideForValidationStepIsRecorded(string validationStep)
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = await connection.GetAllAsync<PendingPaymentValidationResult>();
            results.Should().NotBeEmpty();
            results.Where(r => r.Step != validationStep).All(r => r.Result).Should().BeTrue();
            results.Where(r => r.Step != validationStep).All(r => r.OverrideResult.Value).Should().BeFalse();
            results.Where(r => r.Step == validationStep).All(r => r.OverrideResult.Value).Should().BeTrue();
            results.Where(r => r.Step == validationStep).All(r => r.Result).Should().BeFalse();
        }

        [Then(@"the expired validation override is removed")]
        public async Task ThenTheExpiredValidationOverrideIsRemoved()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = await connection.GetAllAsync<ValidationOverride>();
            results.Should().BeEmpty();
        }

        [Then(@"payment records are created")]
        public async Task AndPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = await connection.GetAllAsync<Payment>();
            results = results.Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id);
            results.Should().HaveCount(2);
        }

        [Then(@"no payment records are created")]
        public async Task AndNoPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = await connection.GetAllAsync<Payment>();
            results = results.Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id);
            results.Should().BeEmpty();
        }

        [Then(@"pending payments are not marked as paid")]
        public async Task AndPendingPaymentsAreNotMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id).ToList();
            results.Count.Should().Be(3);
            results.Any(x => x.PaymentMadeDate.HasValue).Should().BeFalse();
        }

        [Then(@"pending payments are marked as paid")]
        public async Task AndPendingPaymentAreMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id &&
                            x.PeriodNumber <= CollectionPeriod)
                .ToList();

            results.Count.Should().Be(2);
            results.All(x => x.PaymentMadeDate.HasValue).Should().BeTrue();
        }

        [Then(@"future payments are not marked as paid")]
        public async Task AndFuturePaymentsAreNotMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id &&
                            x.PeriodNumber > CollectionPeriod);

            results.All(x => x.PaymentMadeDate.HasValue).Should().BeFalse();
        }

        [Then(@"the employed at 365 days check will have a failed validation result")]
        public async Task ThenTheEmployedAt365DaysCheckWillHaveAFailedValidationResult()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == ValidationStep.EmployedAt365Days).ToList();
            results.Count.Should().Be(1);
            results[0].Result.Should().BeFalse();
        }

        [Then(@"the validation result override for the 365 days check is recorded")]
        public async Task ThenTheValidationResultOverrideForThe365DaysCheckIsRecorded() 
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == ValidationStep.EmployedAt365Days).ToList();
            results.Count.Should().Be(1);
            results[0].OverrideResult.Should().BeTrue();
        }

        [Then(@"the 365 day pending payment is not marked as paid")]
        public async Task ThenThe365DayPendingPaymentIsNotMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result.Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id 
                                                                                && x.EarningType == EarningType.SecondPayment).ToList();
            results.Count.Should().Be(1);
            results.Single().PaymentMadeDate.Should().BeNull();
        }

        [Then(@"the 365 day pending payment is marked as paid")]
        public async Task ThenThe365DayPendingPaymentIsMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result.Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id
                && x.EarningType == EarningType.SecondPayment).ToList();
            results.Count.Should().Be(1);
            results.Single().PaymentMadeDate.Should().NotBeNull();
        }

        [Given(@"the ILR submission validation step will fail")]
        public void GivenTheIlrSubmissionValidationStepWillFail()
        {
            GivenTheValidationStepWillFail(ValidationStep.HasIlrSubmission);
        }

        [Given(@"the learner match was unsuccessful")]
        public void GivenTheLearnerMatchWasUnsuccessful()
        {
            GivenTheValidationStepWillFail(ValidationStep.LearnerMatchSuccessful);
        }

        [Then(@"the ILR Submission check will have a failed validation result")]
        public async Task ThenTheIlrSubmissionCheckWillHaveAFailedValidationResult()
        {
            await ThenTheValidationStepWillHaveTwoFailedValidationResult(ValidationStep.HasIlrSubmission);
        }

        [Then(@"the Learner Match Successful check will have a failed validation result")]
        public async Task ThenTheLearnerMatchSuccessfulCheckWillHaveAFailedValidationResult()
        {
            await ThenTheValidationStepWillHaveTwoFailedValidationResult(ValidationStep.LearnerMatchSuccessful);
        }

        [Then(@"no further ILR validation is performed")]
        public async Task ThenNoFurtherIlrValidationIsPerformed()
        {
            var ilrValidationSteps = new[]
            {
                ValidationStep.IsInLearning,
                ValidationStep.HasLearningRecord,
                ValidationStep.HasNoDataLocks,
                ValidationStep.HasDaysInLearning
            };
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result
                .Where(x => ilrValidationSteps.Contains(x.Step));
            results.Any().Should().BeFalse();
        }

        [Given(@"there are payments with sent clawbacks")]
        public void GivenThereArePaymentsWithSentClawbacks()
        {
            _validatePaymentData.AddClawbackPayment(true);
        }

        [Given(@"there is a validation override for '(.*)'")]
        public void GivenThereIsAValidationOverrideFor(string step)
        {
            _validatePaymentData.AddValidationOverride(new ValidationOverrideStep(step, DateTime.Now.AddDays(1)));
        }

        [Given(@"an expired validation override exists for '(.*)'")]
        public void GivenAnExpiredValidationOverrideExistsFor(string step)
        {
            _validatePaymentData.AddValidationOverride(new ValidationOverrideStep(step, DateTime.Now.AddDays(-1)) );
        }
    }
}