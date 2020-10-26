using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
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
    public class ValidatePaymentsSteps
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture = new Fixture();
        private Account _accountModel;
        private PendingPayment _pendingPayment1;
        private IncentiveApplication _applicationModel;
        private List<IncentiveApplicationApprenticeship> _apprenticeshipsModels;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private PendingPayment _pendingPayment2;
        private const int NumberOfApprenticeships = 3;
        private const short CollectionPeriodYear = 2021;
        private const byte CollectionPeriodMonth = 6;

        public ValidatePaymentsSteps(TestContext testContext)
        {
            _testContext = testContext;
        }

        [Given(@"a legal entity has pending payments without bank details")]
        public async Task GivenALegalEntityDoesNotHaveAValidVendorId()
        {
            _accountModel = _fixture.Create<Account>();
            _accountModel.VrfVendorId = null; // Invalid bank details

            _applicationModel = _fixture.Build<IncentiveApplication>()
                .With(p => p.Status, IncentiveApplicationStatus.InProgress)
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();

            _apprenticeshipsModels = _fixture.Build<IncentiveApplicationApprenticeship>()
                .With(p => p.IncentiveApplicationId, _applicationModel.Id)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .With(p => p.EarningsCalculated, false)
                .CreateMany(NumberOfApprenticeships).ToList();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.IncentiveApplicationApprenticeshipId, _apprenticeshipsModels.First().Id)
                .With(p => p.AccountId, _applicationModel.AccountId)
                .With(p => p.AccountLegalEntityId, _applicationModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipId, _apprenticeshipsModels.First().ApprenticeshipId)
                .With(p => p.PlannedStartDate, DateTime.Today.AddDays(1))
                .With(p => p.DateOfBirth, DateTime.Today.AddYears(-20))
                .Create();

            _pendingPayment1 = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PaymentPeriod, CollectionPeriodMonth)
                .With(p => p.PaymentYear, CollectionPeriodYear)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _pendingPayment2 = _fixture.Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.AccountId, _apprenticeshipIncentive.AccountId)
                .With(p => p.AccountLegalEntityId, _apprenticeshipIncentive.AccountLegalEntityId)
                .With(p => p.PaymentPeriod, CollectionPeriodMonth)
                .With(p => p.PaymentYear, CollectionPeriodYear)
                .Without(p => p.PaymentMadeDate)
                .Create();

            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await connection.InsertAsync(_accountModel);
            await connection.InsertAsync(_applicationModel);
            await connection.InsertAsync(_apprenticeshipsModels);
            await connection.InsertAsync(_apprenticeshipIncentive);
            await connection.InsertAsync(_pendingPayment1);
            await connection.InsertAsync(_pendingPayment2);
        }

        [When(@"the payment process is run")]
        public async Task WhenPendingPaymentsForTheLegalEntityAreValidated()
        {
            var status = await _testContext.PaymentsProcessFunctions.StartPaymentsProcess(CollectionPeriodYear, CollectionPeriodMonth);

            status.RuntimeStatus.Should().NotBe("Failed", status.Output);
            status.RuntimeStatus.Should().Be("Completed");
        }

        [Then(@"the validation fails")]
        public void ThenTheValidationFails()
        {
            // TODO
        }

        [Then(@"validation results are recorded")]
        public async Task ThenValidationResultsAreRecorded()
        {
            await using var connection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var results = connection.GetAllAsync<PendingPaymentValidationResult>().Result.ToList();
            results.Should().HaveCount(2);
            results.Any(r => r.Result == true).Should().BeFalse();
        }

        [Then(@"pending payments are marked as not payable")]
        public void ThenPendingPaymentIsMarkedAsNotPayable()
        {
            // TODO
        }
    }
}
