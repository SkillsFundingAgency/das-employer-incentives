using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.CollectionPeriodDataRepository
{
    public class WhenSave
    {
        private ApprenticeshipIncentives.CollectionPeriodDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;

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
        public async Task Then_existing_collection_period_is_updated()
        {
            // Arrange
            await AddCollectionPeriods();

            var data = (await _sut.GetAll()).ToList();
            data.Single(x => x.Active).SetActive(false);
            data.OrderBy(x => x.PeriodNumber).Last().SetActive(true);

            // Act
            await _sut.Save(data);

            // Assert
            _dbContext.CollectionPeriods.Should().HaveCount(data.Count);
            _dbContext.CollectionPeriods.Single(x => x.PeriodNumber == 3).Active.Should().BeTrue();
        }

        [Test]
        public async Task Then_new_collection_period_is_not_added()
        {
            // Arrange
            await AddCollectionPeriods();

            var data = (await _sut.GetAll()).ToList();
            data.Add(new CollectionPeriod(1, 1, 2030, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), 2030, false));

            // Act
            await _sut.Save(data);

            // Assert
            _dbContext.CollectionPeriods.Should().HaveCount(data.Count - 1);
            _dbContext.CollectionPeriods.Any(x => x.AcademicYear == "2030").Should().BeFalse();
        }

        [Test]
        public async Task Then_removed_collection_period_is_not_deleted()
        {
            // Arrange
            await AddCollectionPeriods();

            var data = (await _sut.GetAll()).ToList();
            data.RemoveAt(1);

            // Act
            await _sut.Save(data);

            // Assert
            _dbContext.CollectionPeriods.Should().HaveCount(data.Count + 1);
            _dbContext.CollectionPeriods.Any(x => x.AcademicYear == "2030").Should().BeFalse();
        }

        private async Task AddCollectionPeriods()
        {
            var collectionPeriod = new[]
            {
                _fixture.Build<ApprenticeshipIncentives.Models.CollectionPeriod>()
                    .With(x => x.Active, false)
                    .With(x => x.PeriodNumber, 1)
                    .With(x => x.CalendarMonth, 8)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 8, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 8, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create(),
                _fixture.Build<ApprenticeshipIncentives.Models.CollectionPeriod>()
                    .With(x => x.Active, true)
                    .With(x => x.PeriodNumber, 2)
                    .With(x => x.CalendarMonth, 9)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create(),
                _fixture.Build<ApprenticeshipIncentives.Models.CollectionPeriod>()
                    .With(x => x.Active, false)
                    .With(x => x.PeriodNumber, 3)
                    .With(x => x.CalendarMonth, 10)
                    .With(x => x.CalendarYear, 2020)
                    .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 10, 6))
                    .With(x => x.CensusDate, new DateTime(2020, 10, 30))
                    .With(x => x.AcademicYear, "2021")
                    .Create()
            };
            await _dbContext.AddRangeAsync(collectionPeriod);
            await _dbContext.SaveChangesAsync();
        }
    }
}
