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
    public class WhenArchivePendingPaymentModelCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_pending_payment_model_and_its_validation_results_are_added_to_the_data_store()
        {
            // Arrange
            var testPendingPaymentValidationResult = _fixture.Create<PendingPaymentValidationResultModel>();

            var testPendingPayment = _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>(){ testPendingPaymentValidationResult })
                .Create();

            // Act
            await _sut.Archive(testPendingPayment);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ArchivedPendingPayments.Count().Should().Be(1);
            _dbContext.ArchivedPendingPaymentValidationResults.Count().Should().Be(1);

            var storedPendingPayment = _dbContext.ArchivedPendingPayments.Single();
            storedPendingPayment.PendingPaymentId.Should().Be(testPendingPayment.Id);
            storedPendingPayment.AccountId.Should().Be(testPendingPayment.Account.Id);
            storedPendingPayment.AccountLegalEntityId.Should().Be(testPendingPayment.Account.AccountLegalEntityId);
            storedPendingPayment.ApprenticeshipIncentiveId.Should().Be(testPendingPayment.ApprenticeshipIncentiveId);
            storedPendingPayment.Amount.Should().Be(testPendingPayment.Amount);
            storedPendingPayment.DueDate.Should().Be(testPendingPayment.DueDate);
            storedPendingPayment.CalculatedDate.Should().Be(testPendingPayment.CalculatedDate);
            storedPendingPayment.PaymentMadeDate.Should().Be(testPendingPayment.PaymentMadeDate);
            storedPendingPayment.PeriodNumber.Should().Be(testPendingPayment.CollectionPeriod.PeriodNumber);
            storedPendingPayment.PaymentYear.Should().Be(testPendingPayment.CollectionPeriod.AcademicYear);
            storedPendingPayment.EarningType.Should().Be(testPendingPayment.EarningType);
            storedPendingPayment.ClawedBack.Should().Be(testPendingPayment.ClawedBack);
            storedPendingPayment.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

            var storedValidationResult = _dbContext.ArchivedPendingPaymentValidationResults.Single();
            storedValidationResult.PendingPaymentId.Should().Be(testPendingPayment.Id);
            storedValidationResult.PendingPaymentValidationResultId.Should().Be(testPendingPaymentValidationResult.Id);
            storedValidationResult.PaymentYear.Should().Be(testPendingPaymentValidationResult.CollectionPeriod.AcademicYear);
            storedValidationResult.PeriodNumber.Should().Be(testPendingPaymentValidationResult.CollectionPeriod.PeriodNumber);
            storedValidationResult.Result.Should().Be(testPendingPaymentValidationResult.Result);
            storedValidationResult.Step.Should().Be(testPendingPaymentValidationResult.Step);
            storedValidationResult.CreatedDateUtc.Should().Be(testPendingPaymentValidationResult.CreatedDateUtc);
            storedValidationResult.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }
}
