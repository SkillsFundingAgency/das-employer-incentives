using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using TechTalk.SpecFlow;
using ApprenticeshipIncentive = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ApprenticeshipIncentive;
using Learner = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Learner;
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
        private readonly Fixture _fixture = new Fixture();
        private Account _accountModel;
        private PendingPayment _pendingPayment1;
        private PendingPayment _pendingPayment2;
        private PendingPayment _pendingPayment3;
        private IncentiveApplication _applicationModel;
        private Learner _learner;
        private List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private const int NumberOfApprenticeships = 3;
        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriod = 6;
        private bool _isInLearning;
        private bool _hasBankDetails;

        public ValidatePaymentsSteps(TestContext testContext)
        {
            _accountModel = _fixture.Build<Account>().Without(a => a.VrfVendorId).Create();
            _isInLearning = true;
            _hasBankDetails = true;
            _testContext = testContext;
        }

        [Given(@"there are pending payments")]
        public Task GivenThereArePendingPayments()
        {
            return CreateIncentiveWithPayments();
        }

        [Given(@"the '(.*)' will fail")]
        public void GivenTheValidationStepWillFail(string validationStep)
        {
            switch (validationStep)
            {
                case ValidationStep.HasBankDetails:
                    _hasBankDetails = false;
                    break;
                case ValidationStep.IsInLearning:
                    _isInLearning = false;
                    break;
            }
        }

        [When(@"the payment process is run")]
        public async Task WhenThePaymentProcessIsRun()
        {
            await CreateAccount();
            await CreateLearnerRecord();

            var status = await _testContext.PaymentsProcessFunctions.StartPaymentsProcess(CollectionPeriodYear, CollectionPeriod);

            status.RuntimeStatus.Should().NotBe("Failed", status.Output);
            status.RuntimeStatus.Should().Be("Completed");
        }

        [Then(@"the '(.*)' will have a failed validation result")]
        public async Task ThenTheValidationStepWillHaveAFailedValidationResult(string step)
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.Where(x => x.Step == step).ToList();
            results.Should().HaveCount(2);
            results.All(r => r.Result == false).Should().BeTrue();
        }

        [Then(@"successful validation results are recorded")]
        public async Task ThenSuccessValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.ToList();
            results.All(r => r.Result == true).Should().BeTrue();
        }

        [Then(@"payment records are created")]
        public async Task AndPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Payment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);
            results.Should().HaveCount(2);
        }

        [Then(@"no payment records are created")]
        public async Task AndNoPaymentRecordIsCreated()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<Payment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);
            results.Should().BeEmpty();
        }

        [Then(@"pending payments are not marked as paid")]
        public async Task AndPendingPaymentsAreNotMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();
            results.Count.Should().Be(3);
            results.Any(x => x.PaymentMadeDate.HasValue).Should().BeFalse();
        }

        [Then(@"pending payments are marked as paid")]
        public async Task AndPendingPaymentAreMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id &&
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
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id &&
                            x.PeriodNumber > CollectionPeriod);

            results.All(x => x.PaymentMadeDate.HasValue).Should().BeFalse();
        }
    }
}