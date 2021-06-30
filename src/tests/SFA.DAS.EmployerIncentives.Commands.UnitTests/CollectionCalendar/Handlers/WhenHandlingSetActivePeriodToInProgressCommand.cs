﻿using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CollectionCalendar.Handlers
{
    [TestFixture]
    public class WhenHandlingSetActivePeriodToInProgressCommand
    {
        private SetActivePeriodToInProgressCommandHandler _sut;
        private Mock<ICollectionCalendarService> _service;
        private Domain.ValueObjects.CollectionPeriod _activeCollectionPeriod;
        private Domain.ValueObjects.CollectionCalendar _collectionCalendar;
        private Domain.ValueObjects.CollectionPeriod _previousCollectionPeriod;

        [SetUp]
        public void Arrange()
        {
            _service = new Mock<ICollectionCalendarService>();
            _sut = new SetActivePeriodToInProgressCommandHandler(_service.Object);

            _previousCollectionPeriod = new Domain.ValueObjects.CollectionPeriod(1, 2021);
            _previousCollectionPeriod.SetPeriodEndInProgress(true);

            _activeCollectionPeriod = new Domain.ValueObjects.CollectionPeriod(2, 2021);
            _activeCollectionPeriod.SetActive(true);

            var calendarPeriods = new List<Domain.ValueObjects.CollectionPeriod>
            {
                _previousCollectionPeriod,
                _activeCollectionPeriod,
                new Domain.ValueObjects.CollectionPeriod(3, 2021)
            };

            _collectionCalendar = new Domain.ValueObjects.CollectionCalendar(calendarPeriods);
            _service.Setup(x => x.Get()).ReturnsAsync(_collectionCalendar);
        }

        [Test]
        public async Task Then_the_active_period_is_set_to_in_progress()
        {
            // Arrange
            var command = new SetActivePeriodToInProgressCommand();

            // Act
            await _sut.Handle(command);

            // Assert
            _activeCollectionPeriod.PeriodEndInProgress.Should().BeTrue();
            _previousCollectionPeriod.PeriodEndInProgress.Should().BeFalse();
            _service.Verify(x => x.Save(_collectionCalendar), Times.Once);
        }
    }
}
