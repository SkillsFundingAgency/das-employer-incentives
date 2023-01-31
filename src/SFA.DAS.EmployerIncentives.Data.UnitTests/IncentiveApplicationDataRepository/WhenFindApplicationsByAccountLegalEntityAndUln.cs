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
    public class WhenFindApplicationsByAccountLegalEntityAndUln
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
        public async Task Then_the_incentive_application_is_returned()
        {
            // Arrange
            var apps = _fixture.CreateMany<Models.IncentiveApplication>(3).ToList();
            apps[0].AccountLegalEntityId = 123;
            apps[0].Apprenticeships.First().ULN = 12345678;

            apps[1].AccountLegalEntityId = 456;
            apps[1].Apprenticeships.First().ULN = 12345678;

            apps[2].AccountLegalEntityId = 123;
            foreach (var apprenticeship in apps[2].Apprenticeships)
            {
                apprenticeship.ULN = 9877654;
            }

            await _dbContext.AddRangeAsync(apps);
            await _dbContext.SaveChangesAsync();

            // Act
            var actual = await _sut.FindApplicationsByAccountLegalEntityAndUln(123, 12345678);

            // Assert
            actual.Should().BeEquivalentTo(new[] { apps[0] }, opt => opt.ExcludingMissingMembers());
        }
    }
}
