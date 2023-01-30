using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;
using Moq;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    [NonParallelizable]
    public class WhenFindApprenticeshipIncentivesWithoutPendingPayments
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private List<ApprenticeshipIncentives.Models.ApprenticeshipIncentive> _incentives;
        private (Guid IncentiveWithPendingPayments, ApprenticeshipIncentives.Models.ApprenticeshipIncentive[] IncentivesWithoutPendingPyaments) _expected;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public async Task SetUp()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));

            var cpData = _fixture.Build<CollectionCalendarPeriod>()
                .With(x => x.Active, true)
                .With(x => x.PeriodNumber, 2)
                .With(x => x.CalendarMonth, 9)
                .With(x => x.CalendarYear, 2020)
                .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                .With(x => x.AcademicYear, "2021")
                .Create();

            await _dbContext.AddAsync(cpData);

            _incentives = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.Status, IncentiveStatus.Active)
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .CreateMany(3).ToList();

            _expected = (IncentiveWithPendingPayments: _incentives[0].Id,
                IncentivesWithoutPendingPyaments: new[] { _incentives[1], _incentives[2] });

            var pendingPayments = _fixture
                .Build<PendingPayment>()
                .With(p => p.ApprenticeshipIncentiveId, _expected.IncentiveWithPendingPayments)
                .CreateMany(2).ToList();

            await _dbContext.AddRangeAsync(pendingPayments);
            await _dbContext.AddRangeAsync(_incentives);
            await _dbContext.SaveChangesAsync();
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_expected_data_returned()
        {
            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments();

            // Assert
            result.Select(x => x.Id).Should().BeEquivalentTo(_expected.IncentivesWithoutPendingPyaments.Select(x => x.Id));
        }

        [Test]
        public async Task Then_stopped_incentives_are_not_returned_when_include_stopped_is_false()
        {
            // Arrange
            var stoppedIncentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.Status, IncentiveStatus.Stopped)
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .Create();
            await _dbContext.AddAsync(stoppedIncentive);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments(false);

            // Assert
            result.Should().NotContain(x => x.Id == stoppedIncentive.Id);
        }

        [Test]
        public async Task Then_stopped_incentives_are_returned_when_include_stopped_is_true()
        {
            // Arrange
            var stoppedIncentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.Status, IncentiveStatus.Stopped)
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .Create();
            await _dbContext.AddAsync(stoppedIncentive);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments(true);

            // Assert
            result.Should().Contain(x => x.Id == stoppedIncentive.Id);
        }

        [Test]
        public async Task Then_withdrawn_incentives_are_not_returned_when_include_withdrawn_is_false()
        {
            // Arrange
            var withdrawnIncentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.Status, IncentiveStatus.Withdrawn)
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .Create();
            await _dbContext.AddAsync(withdrawnIncentive);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments(includeWithdrawn: false);

            // Assert
            result.Should().NotContain(x => x.Id == withdrawnIncentive.Id);
        }

        [Test]
        public async Task Then_withdrawn_incentives_are_returned_when_include_withdrawn_is_true()
        {
            // Arrange
            var withdrawnIncentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(x => x.Status, IncentiveStatus.Withdrawn)
                .Without(x => x.PendingPayments)
                .Without(x => x.BreakInLearnings)
                .Create();
            await _dbContext.AddAsync(withdrawnIncentive);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.FindApprenticeshipIncentivesWithoutPendingPayments(includeWithdrawn: true);

            // Assert
            result.Should().Contain(x => x.Id == withdrawnIncentive.Id);
        }
    }
}
