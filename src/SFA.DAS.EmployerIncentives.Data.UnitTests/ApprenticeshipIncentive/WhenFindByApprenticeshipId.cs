using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenFindByApprenticeshipId
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_apprenticeship_incentive_is_returned_if_exists()
        {
            // Arrange
            var expected = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().Without(x => x.BreakInLearnings).Create();
            await _dbContext.AddAsync(expected);
            await _dbContext.SaveChangesAsync();

            // Act
            var incentive = await _sut.FindByApprenticeshipId(expected.IncentiveApplicationApprenticeshipId);

            // Assert
            incentive.Should().NotBeNull();
        }

        [Test]
        public async Task Then_null_is_returned_if_apprenticeship_incentive_does_not_exist()
        {
            // Act
            var result = await _sut.FindByApprenticeshipId(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }
    }
}
