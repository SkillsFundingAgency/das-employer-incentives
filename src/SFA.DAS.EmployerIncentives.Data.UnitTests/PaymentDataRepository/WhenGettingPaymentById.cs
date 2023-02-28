using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PaymentDataRepository
{
    [TestFixture]
    public class WhenGettingPaymentById
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
        public async Task Then_the_payment_is_returned_if_one_matching_the_payment_id_is_found()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var payment = _fixture.Build<Payment>()
                .With(x => x.Id, paymentId)
                .Create();

            await _dbContext.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.Get(paymentId);

            // Assert
            result.Id.Should().Be(paymentId);
            result.PendingPaymentId.Should().Be(payment.PendingPaymentId);
            result.ApprenticeshipIncentiveId.Should().Be(payment.ApprenticeshipIncentiveId);
        }

        [Test]
        public async Task Then_a_null_value_is_returned_if_a_payment_matching_the_payment_id_cannot_be_found()
        {
            // Arrange
            var paymentId = Guid.NewGuid();
            var payment = _fixture.Build<Payment>()
                .With(x => x.Id, paymentId)
                .Create();

            await _dbContext.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.Get(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
    }
}
