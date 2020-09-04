using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(_dbContext);
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_added_to_the_data_store()
        {
            // Arrange
            var testIncentive = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Create();

            // Act
            await _sut.Add(testIncentive);

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(1);

            var storedAccount = _dbContext.ApprenticeshipIncentives.Single();
            storedAccount.Id.Should().Be(testIncentive.Id);
            storedAccount.AccountId.Should().Be(testIncentive.Account.Id);
            storedAccount.ApprenticeshipId.Should().Be(testIncentive.Apprenticeship.Id);
            storedAccount.FirstName.Should().Be(testIncentive.Apprenticeship.FirstName);
            storedAccount.LastName.Should().Be(testIncentive.Apprenticeship.LastName);
            storedAccount.Uln.Should().Be(testIncentive.Apprenticeship.UniqueLearnerNumber);
            storedAccount.DateOfBirth.Should().Be(testIncentive.Apprenticeship.DateOfBirth);
            storedAccount.EmployerType.Should().Be(testIncentive.Apprenticeship.EmployerType);
        }
    }
}
