using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationDataRepository
{
    [TestFixture]
    public class WhenFindApplicationsByAccountLegalEntity
    {
        private IIncentiveApplicationDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _mockServiceProvider = new Mock<IServiceProvider>();

            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new IncentiveApplication.IncentiveApplicationDataRepository(
                new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_incentive_applications_for_the_account_legal_entity_are_returned()
        {
            // Arrange
            var apps = _fixture.CreateMany<Models.IncentiveApplication>(3).ToList();
            apps[0].AccountLegalEntityId = 123;
            apps[1].AccountLegalEntityId = 456;
            apps[2].AccountLegalEntityId = 123;

            await _dbContext.AddRangeAsync(apps);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApplicationsByAccountLegalEntity(123);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
        }

        [Test]
        public async Task Then_no_applications_are_returned_if_none_match_the_account_legal_entity()
        {
            // Arrange
            var apps = _fixture.CreateMany<Models.IncentiveApplication>(3).ToList();
            apps[0].AccountLegalEntityId = 900;
            apps[1].AccountLegalEntityId = 800;
            apps[2].AccountLegalEntityId = 700;

            await _dbContext.AddRangeAsync(apps);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApplicationsByAccountLegalEntity(123);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
