using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PaymentDataRepository
{
    public class WhenRecordingPaymentHaveBeenSent
    {
        private ApprenticeshipIncentives.PaymentDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.PaymentDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_payment_paid_date_is_updated_for_correct_payments()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var account = new Models.Account { LegalEntityName = _fixture.Create<string>(), AccountLegalEntityId = accountLegalEntityId, VrfVendorId = _fixture.Create<string>() };
            var payments = _fixture
                .Build<Payment>()
                .Without(p => p.PaidDate)
                .CreateMany(5).ToList();
            foreach(var payment in payments)
            {
                payment.AccountLegalEntityId = accountLegalEntityId;
            }

            await _dbContext.AddAsync(account);
            await _dbContext.AddRangeAsync(payments);
            await _dbContext.SaveChangesAsync();

            var paymentIds = payments.Take(4).Select(p => p.Id).ToList();
            var expected = _fixture.Create<DateTime>();

            // Act
            await _sut.RecordPaymentsSent(paymentIds, accountLegalEntityId, expected);

            // Assert
            var matching = _dbContext.Payments.Where(p => paymentIds.Contains(p.Id));

            matching.Count().Should().Be(4);

            foreach (var payment in matching)
            {
                payment.PaidDate.Should().Be(expected);
            }

            var nonMatching = _dbContext.Payments.Where(p => !paymentIds.Contains(p.Id));

            nonMatching.Count().Should().Be(1);

            foreach (var payment in nonMatching)
            {
                payment.PaidDate.Should().BeNull();
            }
        }

        [Test]
        public async Task Then_the_vendor_id_is_recorded_with_the_updated_payment()
        {
            // Arrange
            var accountLegalEntityId = _fixture.Create<long>();
            var account = new Models.Account { LegalEntityName = _fixture.Create<string>(), AccountLegalEntityId = accountLegalEntityId, VrfVendorId = _fixture.Create<string>()};
            var payments = _fixture
                .Build<Payment>()
                .Without(p => p.PaidDate)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .CreateMany(5).ToList();
            payments.First().AccountLegalEntityId = accountLegalEntityId + 1;

            await _dbContext.AddAsync(account);
            await _dbContext.AddRangeAsync(payments);
            await _dbContext.SaveChangesAsync();

            var paymentIds = payments.Take(4).Select(p => p.Id).ToList();
            var expected = _fixture.Create<DateTime>();

            // Act
            await _sut.RecordPaymentsSent(paymentIds, accountLegalEntityId, expected);

            // Assert
            var matching = _dbContext.Payments.Where(p =>
                paymentIds.Contains(p.Id) && p.AccountLegalEntityId == accountLegalEntityId);
            matching.Count().Should().Be(3);
            foreach (var payment in matching)
            {
                payment.VrfVendorId.Should().Be(account.VrfVendorId);
            }
        }
    }
}
