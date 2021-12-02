using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EmploymentCheck
{
    [TestFixture]
    public class WhenRefreshingEmploymentCheck
    {
        private EmploymentCheckController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Fixture _fixture;
        
        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), true, false)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods));

            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _sut = new EmploymentCheckController(_mockCommandDispatcher.Object, _mockCollectionCalendarService.Object);
        }

        [Test]
        public async Task Then_a_refresh_employment_checks_command_is_dispatched()
        {
            // Arrange / Act
            await _sut.Refresh();

            // Assert
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<RefreshEmploymentChecksCommand>(),  It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_an_Ok_response_is_returned()
        {
            // Arrange / Act
            var actual = await _sut.Refresh() as OkResult;

            // Assert
            actual.Should().NotBeNull();
        }

        [Test]
        public async Task Then_no_earning_resilience_check_command_is_dispatched_when_the_active_period_is_in_progress()
        {
            // Arrange
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), true, true)
            };
            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods));

            // Act
            await _sut.Refresh();

            // Assert
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<RefreshEmploymentChecksCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}