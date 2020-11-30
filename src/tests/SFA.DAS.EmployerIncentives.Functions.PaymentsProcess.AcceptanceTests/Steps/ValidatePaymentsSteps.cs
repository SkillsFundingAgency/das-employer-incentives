using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

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
        private List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private const int NumberOfApprenticeships = 3;
        private const short PaymentYear = 2021;
        private const byte CollectionPeriod = 6;

        public ValidatePaymentsSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"a legal entity has pending payments with valid bank details")]
        public async Task GivenALegalEntityHasAValidVendorId()
        {
            await CreateIncentiveWithPayments("ABC123");
        }

        [Given(@"a legal entity has pending payments without bank details")]
        public async Task GivenALegalEntityDoesNotHaveAValidVendorId()
        {
            await CreateIncentiveWithPayments();
        }

        [When(@"the payment process is run")]
        public async Task WhenPendingPaymentsForTheLegalEntityAreValidated()
        {
            var status =
                await _testContext.PaymentsProcessFunctions.StartPaymentsProcess(PaymentYear,
                    CollectionPeriod);

            status.RuntimeStatus.Should().NotBe("Failed", status.Output);
            status.RuntimeStatus.Should().Be("Completed");
        }

        [Then(@"successful validation results are recorded")]
        public async Task ThenSuccessValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.ToList();
            results.Should().HaveCount(2);
            results.Any(r => r.Result == false).Should().BeFalse();

            var bankDetailsValidationResult = results.First(x => x.Step == "HasBankDetails");
            bankDetailsValidationResult.Should().NotBeNull("Should have a bank details validation result");
            bankDetailsValidationResult.PeriodNumber.Should().Be(CollectionPeriod);
            bankDetailsValidationResult.PaymentYear.Should().Be(PaymentYear);
            bankDetailsValidationResult.CreatedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(3));
            bankDetailsValidationResult.Result.Should().BeTrue();
        }

        [Then(@"failed validation results are recorded")]
        public async Task ThenFailedValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.ToList();
            results.Should().HaveCount(2);
            results.Any(r => r.Result == true).Should().BeFalse();

            var bankDetailsValidationResult = results.First(x => x.Step == "HasBankDetails");
            bankDetailsValidationResult.Should().NotBeNull("Should have a bank details validation result");
            bankDetailsValidationResult.PeriodNumber.Should().Be(CollectionPeriod);
            bankDetailsValidationResult.PaymentYear.Should().Be(PaymentYear);
            bankDetailsValidationResult.CreatedDateUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(3));
            bankDetailsValidationResult.Result.Should().BeFalse();
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
            var payments = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id &&
                            x.PeriodNumber <= CollectionPeriod)
                .ToList();

            payments.Count.Should().Be(2);
            payments.All(x => x.PaymentMadeDate.HasValue).Should().BeTrue();
        }

        [Then(@"future payments are not marked as paid")]
        public async Task AndFuturePaymentsAreNotMarkedAsPaid()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var payments = connection.GetAllAsync<PendingPayment>().Result
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id &&
                            x.PeriodNumber > CollectionPeriod).ToArray();

            payments.All(x => x.PaymentMadeDate.HasValue).Should().BeFalse();
        }
    }
}