using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenFindApprenticeshipIncentivesWithoutPendingPayments
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_expected_data_returned()
        {
            // Arrange
            var incentives = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .CreateMany(3).ToList();

            var expected = (IncentiveWithPendingPayments: incentives[0].Id,
                IncentivesWithoutPendingPyaments: new[] { incentives[1], incentives[2] });

            var cpData = _fixture.Build<CollectionCalendarPeriod>()
                .With(x => x.Active, true)
                .With(x => x.PeriodNumber, 2)
                .With(x => x.CalendarMonth, 9)
                .With(x => x.CalendarYear, 2020)
                .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                .With(x => x.AcademicYear, "2021")
                .Create();

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, expected.IncentiveWithPendingPayments)
                .CreateMany(2).ToList();

            await _dbContext.AddAsync(cpData);
            await _dbContext.AddRangeAsync(pendingPayments);
            await _dbContext.AddRangeAsync(incentives);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments();

            // Assert
            result.Select(x => x.Id).Should().BeEquivalentTo(expected.IncentivesWithoutPendingPyaments.Select(x => x.Id));
        }
    }
}
