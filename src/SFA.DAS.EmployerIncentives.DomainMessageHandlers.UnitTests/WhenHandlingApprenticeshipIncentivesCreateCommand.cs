using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests
{

    [TestFixture]
    public class WhenHandlingApprenticeshipIncentivesCreateCommand
    {
        private HandleApprenticeshipIncentivesCreateCommand _sut;
        private Mock<ICommandService> _mockCommandService;
        private Fixture _fixture;
        private CreateApprenticeshipIncentiveCommand _command;

        [SetUp]
        public void Arrange()
        {
            _mockCommandService = new Mock<ICommandService>();
            _fixture = new Fixture();
            _command = _fixture.Create<CreateApprenticeshipIncentiveCommand>();
            _sut = new HandleApprenticeshipIncentivesCreateCommand(_mockCommandService.Object);
        }

        [Test]
        public async Task Then_ensure_command_is_dispatched()
        {
            await _sut.HandleCommand(_command);
            _mockCommandService.Verify(x => x.Dispatch(_command));
        }
    }
}
