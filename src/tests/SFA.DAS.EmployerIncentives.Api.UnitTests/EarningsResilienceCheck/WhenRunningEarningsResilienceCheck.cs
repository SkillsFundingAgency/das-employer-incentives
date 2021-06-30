using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EarningsResilienceCheck
{
    [TestFixture]
    public class WhenRunningEarningsResilienceCheck
    {
        private EarningsResilienceCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private List<Domain.ValueObjects.CollectionPeriod> _collectionPeriods;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), _fixture.Create<short>(),true, false)
            };

            _mockCollectionCalendarService
               .Setup(m => m.Get())
               .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(_collectionPeriods));

            _sut = new EarningsResilienceCommandController(_mockCommandDispatcher.Object, _mockCollectionCalendarService.Object);

            _mockCommandDispatcher
                .Setup(m => m.SendMany(It.IsAny<List<ICommand>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_an_earning_resilience_check_command_is_dispatched()
        {
            // Act
            await _sut.CheckApplications();

            // Assert
            _mockCommandDispatcher.Verify(m => m.SendMany(It.IsAny<List<ICommand>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Then_no_earning_resilience_check_command_is_dispatched_when_the_active_period_is_in_progress()
        {
            // Arrange
            _collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), _fixture.Create<short>(),true, true)
            };
            _mockCollectionCalendarService
               .Setup(m => m.Get())
               .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(_collectionPeriods));

            // Act
            await _sut.CheckApplications();

            // Assert
            _mockCommandDispatcher.Verify(m => m.SendMany(It.IsAny<List<ICommand>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
