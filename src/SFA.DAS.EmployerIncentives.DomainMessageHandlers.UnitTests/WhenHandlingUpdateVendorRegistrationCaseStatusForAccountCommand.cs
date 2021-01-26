using System.Threading;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests
{

    [TestFixture]
    public class WhenHandlingUpdateVendorRegistrationCaseStatusForAccountCommand
    {
        private HandleUpdateVendorRegistrationCaseStatusForAccountCommand _sut;
        private Mock<ICommandDispatcher> _mockCommandService;
        private Fixture _fixture;
        private UpdateVendorRegistrationCaseStatusForAccountCommand _command;

        [SetUp]
        public void Arrange()
        {
            _mockCommandService = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _command = _fixture.Create<UpdateVendorRegistrationCaseStatusForAccountCommand>();
            _sut = new HandleUpdateVendorRegistrationCaseStatusForAccountCommand(_mockCommandService.Object);
        }

        [Test]
        public async Task Then_ensure_command_is_dispatched()
        {
            await _sut.HandleCommand(_command);
            _mockCommandService.Verify(x => x.Send(_command, It.IsAny<CancellationToken>()));
        }
    }
}
