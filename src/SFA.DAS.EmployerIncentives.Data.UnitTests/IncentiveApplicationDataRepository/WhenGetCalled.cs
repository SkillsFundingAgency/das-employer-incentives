using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationDataRepository
{
    public class WhenGetCalled
    {
        private IncentiveApplication.IncentiveApplicationDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new IncentiveApplication.IncentiveApplicationDataRepository(
                new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_incentive_application_is_returned()
        {
            // Arrange
            var expected = _fixture.Create<Models.IncentiveApplication>();
            await _dbContext.AddAsync(expected);
            await _dbContext.SaveChangesAsync();

            // Act
            var actual = await _sut.Get(expected.Id);

            // Assert
            actual.Should().BeEquivalentTo(expected, opt => opt
                .ExcludingMissingMembers());
        }
        [Test]
        public async Task Then_null_is_returned_when_application_does_not_exist()
        {
            // Act
            var actual = await _sut.Get(Guid.Empty);

            // Assert
            actual.Should().BeNull();
        }
    }
}
