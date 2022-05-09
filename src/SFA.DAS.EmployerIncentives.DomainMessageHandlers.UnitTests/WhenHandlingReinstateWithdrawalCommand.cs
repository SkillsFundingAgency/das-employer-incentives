using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;

namespace SFA.DAS.EmployerIncentives.DomainMessageHandlers.UnitTests
{
    [TestFixture]
    public class WhenHandlingReinstateWithdrawalCommand
    {
        private HandleReinstateWithdrawalCommand _sut;
        private Mock<ICommandService> _mockCommandService;
        private Fixture _fixture;
        private ReinstateWithdrawalCommand _command;

        [SetUp]
        public void Arrange()
        {
            _mockCommandService = new Mock<ICommandService>();
            _fixture = new Fixture();
            _command = _fixture.Create<ReinstateWithdrawalCommand>();
            _sut = new HandleReinstateWithdrawalCommand(_mockCommandService.Object);
        }

        [Test]
        public async Task Then_ensure_command_is_dispatched()
        {
            await _sut.HandleCommand(_command);
            _mockCommandService.Verify(x=>x.Dispatch(_command));
        }
    }
}
