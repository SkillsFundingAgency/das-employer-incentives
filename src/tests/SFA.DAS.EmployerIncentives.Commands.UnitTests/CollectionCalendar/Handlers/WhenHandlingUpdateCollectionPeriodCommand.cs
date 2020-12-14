using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using SFA.DAS.EmployerIncentives.Commands.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _service = new Mock<ICollectionCalendarService>();
            _sut = new UpdateCollectionPeriodCommandHandler(_service.Object);

            var calendarPeriods = new List<Domain.ValueObjects.CollectionPeriod>
            {
                new Domain.ValueObjects.CollectionPeriod(1, 2020),
                new Domain.ValueObjects.CollectionPeriod(2, 2020),
                new Domain.ValueObjects.CollectionPeriod(3, 2020)
            };

            var collectionCalendar = new Domain.ValueObjects.CollectionCalendar(calendarPeriods);
            _service.Setup(x => x.Get()).ReturnsAsync(collectionCalendar);
        }

        [Test]
        public async Task Then_the_active_period_is_updated()
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(2, 2020, true);

            // Act
            await _sut.Handle(command);

            _service.Verify(x => x.Save(It.IsAny<Domain.ValueObjects.CollectionCalendar>()), Times.Once);
        }

        [Test]
        public async Task Then_the_active_period_is_not_updated_if_active_flag_is_false() 
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(1, 2020, false);

            // Act
            await _sut.Handle(command);

            _service.Verify(x => x.Save(It.IsAny<Domain.ValueObjects.CollectionCalendar>()), Times.Never);
        }
    }
}
