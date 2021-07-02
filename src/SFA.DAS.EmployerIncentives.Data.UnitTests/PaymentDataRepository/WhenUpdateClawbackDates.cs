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
    public class WhenUpdateClawbackDates
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
        public async Task Then_clawback_send_date_is_updated_for_correct_clawbacks()
        {
            // Arrange
            var clawbacks = _fixture
                .Build<ClawbackPayment>()
                .Without(p => p.DateClawbackSent)
                .CreateMany(5).ToList();

            await _dbContext.AddRangeAsync(clawbacks);
            await _dbContext.SaveChangesAsync();

            var clawbacksIds = clawbacks.Take(4).Select(p => p.Id).ToList();
            var expected = _fixture.Create<DateTime>();

            // Act
            await _sut.UpdateClawbackDates(clawbacksIds, expected);

            // Assert
            var matching = _dbContext.ClawbackPayments.Where(p => clawbacksIds.Contains(p.Id));

            matching.Count().Should().Be(4);

            foreach (var clawback in matching)
            {
                clawback.DateClawbackSent.Should().Be(expected);
            }

            var nonMatching = _dbContext.ClawbackPayments.Where(p => !clawbacksIds.Contains(p.Id));

            nonMatching.Count().Should().Be(1);

            foreach (var clawback in nonMatching)
            {
                clawback.DateClawbackSent.Should().BeNull();
            }
        }
    }
}
