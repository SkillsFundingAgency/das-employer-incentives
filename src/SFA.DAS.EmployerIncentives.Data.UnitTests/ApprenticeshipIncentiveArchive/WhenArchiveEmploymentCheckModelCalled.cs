using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentiveArchive
{
    public class WhenArchiveEmploymentCheckModelCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_employment_check_model_is_added_to_the_data_store()
        {
            // Arrange
            var testEmploymentCheck = _fixture
                .Build<EmploymentCheckModel>()
                .Create();

            // Act
            await _sut.Archive(testEmploymentCheck);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ArchivedEmploymentChecks.Count().Should().Be(1);

            var storedEmploymentCheck = _dbContext.ArchivedEmploymentChecks.Single();
            storedEmploymentCheck.EmploymentCheckId.Should().Be(testEmploymentCheck.Id);
            storedEmploymentCheck.ApprenticeshipIncentiveId.Should().Be(testEmploymentCheck.ApprenticeshipIncentiveId);
            storedEmploymentCheck.CheckType.Should().Be(testEmploymentCheck.CheckType.ToString());
            storedEmploymentCheck.CorrelationId.Should().Be(testEmploymentCheck.CorrelationId);
            storedEmploymentCheck.MinimumDate.Should().Be(testEmploymentCheck.MinimumDate);
            storedEmploymentCheck.MaximumDate.Should().Be(testEmploymentCheck.MaximumDate);
            storedEmploymentCheck.Result.Should().Be(testEmploymentCheck.Result);
            storedEmploymentCheck.CreatedDateTime.Should().Be(testEmploymentCheck.CreatedDateTime);
            storedEmploymentCheck.ResultDateTime.Should().Be(testEmploymentCheck.ResultDateTime);
            
            storedEmploymentCheck.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }
}
