using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests
{

    [TestFixture]
    public class WhenHandlingApprenticeshipIncentivesCreateCommand
    {
        private HandleApprenticeshipIncentivesCreateCommand _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;
        private CreateIncentiveCommand _command;

        [SetUp]
        public void Arrange()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _command = _fixture.Create<CreateIncentiveCommand>();
            _sut = new HandleApprenticeshipIncentivesCreateCommand(_mockCommandDispatcher.Object);
        }

        [Test]
        public async Task Then_ensure_command_is_dispatched()
        {
            await _sut.HandleCommand(_command);
            _mockCommandDispatcher.Verify(x => x.Send(_command, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
