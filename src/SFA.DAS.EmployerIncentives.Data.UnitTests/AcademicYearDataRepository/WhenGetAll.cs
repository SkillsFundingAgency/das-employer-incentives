using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.AcademicYearDataRepository
{
    public class WhenGetAll
    {
        private ApprenticeshipIncentives.AcademicYearDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private AcademicYear[] _academicYear;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.AcademicYearDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_all_existing_data_is_returned()
        {
            // Arrange
            await AddCollectionPeriods();

            // Act
            var result = (await _sut.GetAll()).ToList();

            // Assert
            result.Should().BeEquivalentTo(_academicYear, opt => opt
                .Excluding(x => x.Id)
            );

            result.Select(x => x.AcademicYearId).Should()
                .BeEquivalentTo(_academicYear.Select(x => x.Id));
        }
        
        private async Task AddCollectionPeriods()
        {
            _academicYear = new[]
            {
                _fixture.Build<AcademicYear>().Create(),
                _fixture.Build<AcademicYear>().Create()
            };
            await _dbContext.AddRangeAsync(_academicYear);
            await _dbContext.SaveChangesAsync();
        }

    }
}
