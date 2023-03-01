using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.SendSlack;
using SFA.DAS.EmployerIncentives.Commands.Services.SlackApi;
using SFA.DAS.EmployerIncentives.Commands.Types.Notification;
using SFA.DAS.EmployerIncentives.Commands.Types.PaymentProcess;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SendSlack.Handlers
{
    [TestFixture]
    public class WhenHandlingSlackNotificationCommand
    {
        private Mock<ISlackNotificationService> _mockSlackNotificationService;
        private SlackNotificationCommandHandler _sut;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockSlackNotificationService = new Mock<ISlackNotificationService>();
            _sut = new SlackNotificationCommandHandler(_mockSlackNotificationService.Object);
        }

        [Test]
        public async Task Then_the_slack_message_is_sendt_to_the_notification_service()
        {
            // Arrange
            var command = _fixture.Create<SlackNotificationCommand>();

            // Act
            await _sut.Handle(command, new CancellationToken());

            // Assert
            _mockSlackNotificationService.Verify(x => x.Send(It.Is<SlackMessage>(x => x == command.SlackMessage), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
