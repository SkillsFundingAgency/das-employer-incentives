using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(_dbContext);
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_added_to_the_data_store()
        {
            // Arrange
            var testIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Create();

            testIncentive.Apprenticeship.SetProvider(_fixture.Create<Provider>());

            // Act
            await _sut.Add(testIncentive);

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(1);

            var storedApprenticeshipIncentive = _dbContext.ApprenticeshipIncentives.Single();
            storedApprenticeshipIncentive.Id.Should().Be(testIncentive.Id);
            storedApprenticeshipIncentive.AccountId.Should().Be(testIncentive.Account.Id);
            storedApprenticeshipIncentive.ApprenticeshipId.Should().Be(testIncentive.Apprenticeship.Id);
            storedApprenticeshipIncentive.FirstName.Should().Be(testIncentive.Apprenticeship.FirstName);
            storedApprenticeshipIncentive.LastName.Should().Be(testIncentive.Apprenticeship.LastName);
            storedApprenticeshipIncentive.Uln.Should().Be(testIncentive.Apprenticeship.UniqueLearnerNumber);
            storedApprenticeshipIncentive.DateOfBirth.Should().Be(testIncentive.Apprenticeship.DateOfBirth);
            storedApprenticeshipIncentive.EmployerType.Should().Be(testIncentive.Apprenticeship.EmployerType);
            storedApprenticeshipIncentive.UKPRN.Should().Be(testIncentive.Apprenticeship.Provider.Ukprn);
        }

        [Test]
        public async Task Then_the_pending_payments_are_added_to_the_data_store()
        {
            var testPendingPayment = _fixture.Create<PendingPaymentModel>();
            testPendingPayment.PaymentMadeDate = null;

            // Arrange
            var testApprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(f => f.Id, testPendingPayment.ApprenticeshipIncentiveId)
                .With(f => f.PendingPaymentModels, new List<PendingPaymentModel> { testPendingPayment })
                .Create();

            // Act
            await _sut.Add(testApprenticeshipIncentive);

            // Assert
            var storedIncentive = _dbContext.ApprenticeshipIncentives.Single();
            _ = storedIncentive.PendingPayments.Count.Should().Be(1);
            var storedPendingPayment = storedIncentive.PendingPayments.Single();
            storedPendingPayment.Id.Should().Be(testPendingPayment.Id);
            storedPendingPayment.AccountId.Should().Be(testPendingPayment.Account.Id);
            storedPendingPayment.AccountLegalEntityId.Should().Be(testPendingPayment.Account.AccountLegalEntityId);
            storedPendingPayment.ApprenticeshipIncentiveId.Should().Be(testPendingPayment.ApprenticeshipIncentiveId);
            storedPendingPayment.DueDate.Should().Be(testPendingPayment.DueDate);
            storedPendingPayment.Amount.Should().Be(testPendingPayment.Amount);
            storedPendingPayment.CalculatedDate.Should().Be(testPendingPayment.CalculatedDate);
            storedPendingPayment.PaymentMadeDate.Should().Be(testPendingPayment.PaymentMadeDate);
        }

        [Test]
        public async Task Then_the_payments_are_added_to_the_data_store()
        {
            var testPayment = _fixture.Create<PaymentModel>();
            testPayment.PaidDate = null;

            // Arrange
            var testApprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(f => f.Id, testPayment.ApprenticeshipIncentiveId)
                .With(f => f.PaymentModels, new List<PaymentModel> { testPayment })
                .Create();

            // Act
            await _sut.Add(testApprenticeshipIncentive);

            // Assert
            var storedIncentive = _dbContext.ApprenticeshipIncentives.Single();
            _ = storedIncentive.Payments.Count.Should().Be(1);
            var storedPayment = storedIncentive.Payments.Single();
            storedPayment.Id.Should().Be(testPayment.Id);
            storedPayment.AccountId.Should().Be(testPayment.Account.Id);
            storedPayment.AccountLegalEntityId.Should().Be(testPayment.Account.AccountLegalEntityId);
            storedPayment.ApprenticeshipIncentiveId.Should().Be(testPayment.ApprenticeshipIncentiveId);
            storedPayment.PaymentPeriod.Should().Be(testPayment.PaymentPeriod);
            storedPayment.PaymentYear.Should().Be(testPayment.PaymentYear);
            storedPayment.Amount.Should().Be(testPayment.Amount);
            storedPayment.CalculatedDate.Should().Be(testPayment.CalculatedDate);
            storedPayment.PaidDate.Should().Be(testPayment.PaidDate);
            storedPayment.PendingPaymentId.Should().Be(testPayment.PendingPaymentId);
            storedPayment.SubnominalCode.Should().Be(testPayment.SubnominalCode);
        }
    }
}
