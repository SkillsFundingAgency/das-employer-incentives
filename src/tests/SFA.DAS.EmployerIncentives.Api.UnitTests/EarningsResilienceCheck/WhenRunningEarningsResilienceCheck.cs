﻿using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.EarningsResilienceCheck
{
    [TestFixture]
    public class WhenRunningEarningsResilienceCheck
    {
        private EarningsResilienceCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        
        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            
            _sut = new EarningsResilienceCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<ICommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
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
            _mockCommandDispatcher.Verify(m => m.Send(It.IsAny<IncompleteEarningsCalculationCheckCommand>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandDispatcher.Verify(m => m.SendMany(It.Is<List<ICommand>>(x => x.OfType<EarningsResilienceApplicationsCheckCommand>().Any()),
                It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandDispatcher.Verify(m => m.SendMany(It.Is<List<ICommand>>(x => x.OfType<EarningsResilienceIncentivesCheckCommand>().Any()),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
