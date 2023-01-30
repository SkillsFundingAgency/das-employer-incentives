using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ChangeOfCircumstancesDataRepository
{
    public class WhenSaveCalled
    {
        private ApprenticeshipIncentives.ChangeOfCircumstancesDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new ApprenticeshipIncentives.ChangeOfCircumstancesDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_ChangeOfCircumstance_is_added_to_the_data_store()
        {
            // Arrange
            var testChange = _fixture.Create<ChangeOfCircumstance>();

            // Act
            await _sut.Save(testChange);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ChangeOfCircumstances.Count().Should().Be(1);

            var storedChange = _dbContext.ChangeOfCircumstances.Single();
            storedChange.Id.Should().Be(testChange.Id);
            storedChange.ApprenticeshipIncentiveId.Should().Be(testChange.ApprenticeshipIncentiveId);
            storedChange.NewValue.Should().Be(testChange.NewValue);
            storedChange.PreviousValue.Should().Be(testChange.PreviousValue);
            storedChange.ChangedDate.Should().Be(testChange.ChangedDate);
        }
    }
}
