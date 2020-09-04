using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests
{

    [TestFixture]
    public class WhenHandlingApplicationCalculateClaimCommand
    {
        private HandleApplicationCalculateClaimCommand _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ILogger<HandleApplicationCalculateClaimCommand>> _mockLogger;
        private Fixture _fixture;
        private CalculateClaimCommand _command;

        [SetUp]
        public void Arrange()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockLogger = new Mock<ILogger<HandleApplicationCalculateClaimCommand>>();
            _fixture = new Fixture();
            _command = _fixture.Create<CalculateClaimCommand>();
            _sut = new HandleApplicationCalculateClaimCommand(_mockCommandDispatcher.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Then_ensure_command_is_dispatched()
        {
            await _sut.RunEvent(_command);
            _mockCommandDispatcher.Verify(x=>x.Send(_command, It.IsAny<CancellationToken>()));
        }

        [Test]
        public void And_an_exception_occurs_in_CommandDispatcher_Then_ensure_exception_is_logged_and_rethrown()
        {
            _mockCommandDispatcher.Setup(x => x.Send(It.IsAny<CalculateClaimCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ApplicationException("Errored"));
            Assert.ThrowsAsync<ApplicationException>(()=>_sut.RunEvent(_command));
        }
    }
}
