using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    [NonParallelizable]
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

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
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
            
            // Act
            await _sut.Add(testIncentive);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(1);

            var storedApprenticeshipIncentive = _dbContext.ApprenticeshipIncentives.Single();
            storedApprenticeshipIncentive.Id.Should().Be(testIncentive.Id);
            storedApprenticeshipIncentive.AccountId.Should().Be(testIncentive.Account.Id);
            storedApprenticeshipIncentive.ApprenticeshipId.Should().Be(testIncentive.Apprenticeship.Id);
            storedApprenticeshipIncentive.FirstName.Should().Be(testIncentive.Apprenticeship.FirstName);
            storedApprenticeshipIncentive.LastName.Should().Be(testIncentive.Apprenticeship.LastName);
            storedApprenticeshipIncentive.ULN.Should().Be(testIncentive.Apprenticeship.UniqueLearnerNumber);
            storedApprenticeshipIncentive.DateOfBirth.Should().Be(testIncentive.Apprenticeship.DateOfBirth);
            storedApprenticeshipIncentive.EmployerType.Should().Be(testIncentive.Apprenticeship.EmployerType);
            storedApprenticeshipIncentive.UKPRN.Should().Be(testIncentive.Apprenticeship.Provider.Ukprn);
            storedApprenticeshipIncentive.SubmittedDate.Should().Be(testIncentive.SubmittedDate);
            storedApprenticeshipIncentive.SubmittedByEmail.Should().Be(testIncentive.SubmittedByEmail);
            storedApprenticeshipIncentive.CourseName.Should().Be(testIncentive.Apprenticeship.CourseName);
            storedApprenticeshipIncentive.EmploymentStartDate.Should().Be(testIncentive.Apprenticeship.EmploymentStartDate);
            storedApprenticeshipIncentive.Phase.Should().Be(testIncentive.Phase.Identifier);
            storedApprenticeshipIncentive.WithdrawnBy.Should().Be(testIncentive.WithdrawnBy);
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
            await _dbContext.SaveChangesAsync();

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
            await _dbContext.SaveChangesAsync();

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

        [Test]
        public async Task Then_the_employment_checks_are_added_to_the_data_store()
        {
            var testEmploymentCheck = _fixture.Create<EmploymentCheckModel>();
            
            // Arrange
            var testApprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(f => f.Id, testEmploymentCheck.ApprenticeshipIncentiveId)
                .With(f => f.EmploymentCheckModels, new List<EmploymentCheckModel> { testEmploymentCheck })
                .Create();

            // Act
            await _sut.Add(testApprenticeshipIncentive);
            await _dbContext.SaveChangesAsync();

            // Assert
            var storedIncentive = _dbContext.ApprenticeshipIncentives.Single();
            _ = storedIncentive.EmploymentChecks.Count.Should().Be(1);
            var storedEmploymentCheck = storedIncentive.EmploymentChecks.Single();
            storedEmploymentCheck.Id.Should().Be(testEmploymentCheck.Id);
            storedEmploymentCheck.ApprenticeshipIncentiveId.Should().Be(testEmploymentCheck.ApprenticeshipIncentiveId);
            storedEmploymentCheck.CheckType.Should().Be(testEmploymentCheck.CheckType);
            storedEmploymentCheck.CorrelationId.Should().Be(testEmploymentCheck.CorrelationId);
            storedEmploymentCheck.MaximumDate.Should().Be(testEmploymentCheck.MaximumDate);
            storedEmploymentCheck.MinimumDate.Should().Be(testEmploymentCheck.MinimumDate);
            storedEmploymentCheck.Result.Should().Be(testEmploymentCheck.Result);
        }

        [Test]
        public async Task Then_the_validation_overrides_are_added_to_the_data_store()
        {
            var testValidationOverride = _fixture.Create<ValidationOverrideModel>();

            // Arrange
            var testApprenticeshipIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(f => f.Id, testValidationOverride.ApprenticeshipIncentiveId)
                .With(f => f.ValidationOverrideModels, new List<ValidationOverrideModel> { testValidationOverride })
                .Create();

            // Act
            await _sut.Add(testApprenticeshipIncentive);
            await _dbContext.SaveChangesAsync();

            // Assert
            var storedIncentive = _dbContext.ApprenticeshipIncentives.Single();
            _ = storedIncentive.ValidationOverrides.Count.Should().Be(1);
            var storedValidationOverride = storedIncentive.ValidationOverrides.Single();
            storedValidationOverride.Id.Should().Be(testValidationOverride.Id);
            storedValidationOverride.ApprenticeshipIncentiveId.Should().Be(testValidationOverride.ApprenticeshipIncentiveId);
            storedValidationOverride.Step.Should().Be(testValidationOverride.Step);
            storedValidationOverride.ExpiryDate.Should().Be(testValidationOverride.ExpiryDate);
            storedValidationOverride.CreatedDateTime.Should().Be(testValidationOverride.CreatedDate);
        }
    }
}
