using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Types;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Application.UnitTests.Commands.CommandHandlerWithPeriodEndDelay
{
    public class WhenHandleIsCalled
    {
        private CommandHandlerWithPeriodEndDelay<TestCommand> _sut;
        private Mock<ICommandHandler<TestCommand>> _mockHandler;
        private Mock<IScheduledCommandPublisher> _mockScheduledPublisher;
        private Mock<ICollectionCalendarService> __mockCollectionCalendarService;
        private CollectionCalendarPeriod _collectionCalendarPeriod;

        public class DelayableTestCommand : TestCommand, IPeriodEndIncompatible
        {
            public TimeSpan CommandDelay => TimeSpan.FromMinutes(5);

            public bool CancelCommand => false;
        }

        public class CancellableTestCommand : TestCommand, IPeriodEndIncompatible
        {
            public TimeSpan CommandDelay { get; }

            public bool CancelCommand => true;
        }

        public class TestCommand : ICommand { }

        [SetUp]
        public void Arrange()
        {
            _mockHandler = new Mock<ICommandHandler<TestCommand>>();
            _mockScheduledPublisher = new Mock<IScheduledCommandPublisher>();

            _collectionCalendarPeriod = new CollectionCalendarPeriod( new CollectionPeriod(1, 2021), 6, 2021,  DateTime.Now, DateTime.Now, true, false);

            __mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            __mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new CollectionCalendar(new List<AcademicYear>(),  new List<CollectionCalendarPeriod>() { _collectionCalendarPeriod }));

            _sut = new CommandHandlerWithPeriodEndDelay<TestCommand>(_mockHandler.Object, _mockScheduledPublisher.Object, __mockCollectionCalendarService.Object);
        }

        [Test]
        public async Task Then_the_command_is_passed_to_the_handler_when_not_IPeriodEndIncompatible()
        {
            //Arrange
            var command = new TestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
            _mockScheduledPublisher.Verify(m => m.Send(It.IsAny<ICommand>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Then_the_command_is_passed_to_the_handler_when_IPeriodEndIncompatible_and_active_period_month_end_not_in_progress()
        {
            //Arrange
            var command = new DelayableTestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
            _mockScheduledPublisher.Verify(m => m.Send(It.IsAny<ICommand>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Then_the_command_is_delayed_when_IPeriodEndIncompatible_and_active_period_month_end_is_in_progress()
        {
            //Arrange
            var command = new DelayableTestCommand();
            _collectionCalendarPeriod.SetPeriodEndInProgress(true);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockScheduledPublisher.Verify(m => m.Send(command, command.CommandDelay, It.IsAny<CancellationToken>()), Times.Never);
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Then_the_command_is_cancelled_when_IPeriodEndIncompatible_and_active_period_month_end_is_in_progress()
        {
            //Arrange
            var command = new CancellableTestCommand();
            _collectionCalendarPeriod.SetPeriodEndInProgress(true);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockScheduledPublisher.Verify(m => m.Send(command, command.CommandDelay, It.IsAny<CancellationToken>()), Times.Never);
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Then_the_cancellable_command_is_passed_to_the_handler_when_IPeriodEndIncompatible_and_active_period_month_end_not_in_progress()
        {
            //Arrange
            var command = new CancellableTestCommand();

            //Act
            await _sut.Handle(command);

            //Assert
            _mockHandler.Verify(m => m.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
            _mockScheduledPublisher.Verify(m => m.Send(It.IsAny<ICommand>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
