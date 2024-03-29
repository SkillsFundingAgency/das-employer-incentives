﻿using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CollectionCalendar.Handlers
{
    [TestFixture]
    public class WhenHandlingUpdateCollectionPeriodCommand
    {
        private UpdateCollectionPeriodCommandHandler _sut;
        private Mock<ICollectionCalendarService> _service;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _service = new Mock<ICollectionCalendarService>();
            _sut = new UpdateCollectionPeriodCommandHandler(_service.Object);

            var calendarPeriods = new List<Domain.ValueObjects.CollectionCalendarPeriod>
            {
                CollectionPeriod(1, 2021),
                CollectionPeriod(2, 2021),
                CollectionPeriod(3, 2021)
            };

            var collectionCalendar = new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), calendarPeriods);
            _service.Setup(x => x.Get()).ReturnsAsync(collectionCalendar);
        }

        [Test]
        public async Task Then_the_active_period_is_updated()
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(2, 2021, true);

            // Act
            await _sut.Handle(command);

            _service.Verify(x => x.Save(It.IsAny<Domain.ValueObjects.CollectionCalendar>()), Times.Once);
        }

        [Test]
        public async Task Then_the_active_period_is_not_updated_if_active_flag_is_false() 
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(1, 2021, false);

            // Act
            await _sut.Handle(command);

            _service.Verify(x => x.Save(It.IsAny<Domain.ValueObjects.CollectionCalendar>()), Times.Never);
        }

        private Domain.ValueObjects.CollectionCalendarPeriod CollectionPeriod(byte periodNumber, short academicYear)
        {
            return new Domain.ValueObjects.CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(periodNumber, academicYear), 1, academicYear, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, false);
        }
    }
}
