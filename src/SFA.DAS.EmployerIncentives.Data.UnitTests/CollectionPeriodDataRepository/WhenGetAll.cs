using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.CollectionPeriodDataRepository
{
    public class WhenGetAll
    {
        private ApprenticeshipIncentives.CollectionPeriodDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private CollectionPeriod[] _collectionPeriod;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.CollectionPeriodDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
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
            result.Should().BeEquivalentTo(_collectionPeriod, opt => opt
                .Excluding(x => x.Id)
                .Excluding(x => x.AcademicYear)
                .Excluding(x => x.EIScheduledOpenDateUTC)
                .Excluding(x => x.MonthEndProcessingCompleteUTC)
                );

            result.Select(x => x.AcademicYear).Should()
                .BeEquivalentTo(_collectionPeriod.Select(x => Convert.ToInt16(x.AcademicYear)));

            result.Select(x => x.OpenDate).Should()
                .BeEquivalentTo(_collectionPeriod.Select(x => x.EIScheduledOpenDateUTC));
        }

        [Test]
        public async Task Then_null_returned_if_not_data_exist()
        {
            // Act
            var result = await _sut.GetAll();

            // Assert
            result.Should().BeNull();
        }

        private async Task AddCollectionPeriods()
        {
            _collectionPeriod = new[]
            {
                _fixture.Build<CollectionPeriod>()
                    .With(x => x.Active, false)
                    .With(x => x.PeriodEndInProgress, false)
                    .With(x => x.PeriodNumber, 1)
                    .With(x => x.CalendarMonth, 8)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 8, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 8, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create(),
                _fixture.Build<CollectionPeriod>()
                    .With(x => x.Active, true)
                    .With(x => x.PeriodEndInProgress, true)
                    .With(x => x.PeriodNumber, 2)
                    .With(x => x.CalendarMonth, 9)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create(),
                _fixture.Build<CollectionPeriod>()
                    .With(x => x.Active, false)
                    .With(x => x.PeriodEndInProgress, false)
                    .With(x => x.PeriodNumber, 3)
                    .With(x => x.CalendarMonth, 10)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 10, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 10, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create()
            };
            await _dbContext.AddRangeAsync(_collectionPeriod);
            await _dbContext.SaveChangesAsync();
        }

    }
}
