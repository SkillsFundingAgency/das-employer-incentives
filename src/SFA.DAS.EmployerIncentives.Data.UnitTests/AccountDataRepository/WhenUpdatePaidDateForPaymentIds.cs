using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AccountDataRepository
{
    public class WhenUpdatePaidDateForPaymentIds
    {
        private Data.AccountDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new Data.AccountDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
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
            var payments = _fixture
                .Build<Payment>()
                .Without(p => p.PaidDate)
                .With(p => p.AccountLegalEntityId, accountLegalEntityId)
                .CreateMany(5).ToList();
            payments.First().AccountLegalEntityId = accountLegalEntityId + 1;

            await _dbContext.AddRangeAsync(payments);
            await _dbContext.SaveChangesAsync();

            var paymentIds = payments.Take(4).Select(p => p.Id).ToList();
            var expected = _fixture.Create<DateTime>();

            // Act
            await _sut.UpdatePaidDateForPaymentIds(paymentIds, accountLegalEntityId, expected);

            // Assert
            var matching = _dbContext.Payments.Where(p =>
                paymentIds.Contains(p.Id) && p.AccountLegalEntityId == accountLegalEntityId);
            matching.Count().Should().Be(3);
            foreach (var payment in matching)
            {
                payment.PaidDate.Should().Be(expected);
            }

            var nonMatching = _dbContext.Payments.Where(p =>
                !paymentIds.Contains(p.Id) || p.AccountLegalEntityId != accountLegalEntityId);
            nonMatching.Count().Should().Be(2);
            foreach (var payment in nonMatching)
            {
                payment.PaidDate.Should().BeNull();
            }
        }
    }
}
