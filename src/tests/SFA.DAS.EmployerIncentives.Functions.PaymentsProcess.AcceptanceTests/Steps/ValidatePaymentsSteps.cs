using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using Payment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Payment;
using PendingPayment = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPayment;
using PendingPaymentValidationResult = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPaymentValidationResult;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ValidatePayments")]
    public partial class ValidatePaymentsSteps
    {
        private readonly TestContext _testContext;        
        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;

        private ValidatePaymentData _validatePaymentData;

        public ValidatePaymentsSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"there are pending payments")]
        public void GivenThereArePendingPayments()
        {
            _validatePaymentData = new ValidatePaymentData(_testContext);
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
            }
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await _validatePaymentData.Create();

            await _testContext.TestFunction.Start(
               new OrchestrationStarterInfo(
                   "IncentivePaymentOrchestrator_HttpStart",
                   "IncentivePaymentOrchestrator",
                   new Dictionary<string, object>
                   {
                       ["req"] = new DummyHttpRequest
                       {
                           Path = $"/api/orchestrators/IncentivePaymentOrchestrator/{CollectionPeriodYear}/{CollectionPeriod}"
                       },
                       ["collectionPeriodYear"] = CollectionPeriodYear,
                       ["collectionPeriodMonth"] = CollectionPeriod
                   }
                   ));
        }

        [Then(@"the '(.*)' will have a failed validation result")]
        public async Task ThenTheValidationStepWillHaveAFailedValidationResult(string step)
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == step).ToList();
            results.Should().HaveCount(2);
            results.All(r => !r.Result).Should().BeTrue();
        }

        [Then(@"successful validation results are recorded")]
        public async Task ThenSuccessValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.ToList();
            results.All(r => r.Result).Should().BeTrue();
        }

        [Then(@"payment records are created")]
        public async Task AndPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Payment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id);
            results.Should().HaveCount(2);
        }

        [Then(@"no payment records are created")]
        public async Task AndNoPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Payment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _validatePaymentData.ApprenticeshipIncentiveModel.Id);
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
    }
}