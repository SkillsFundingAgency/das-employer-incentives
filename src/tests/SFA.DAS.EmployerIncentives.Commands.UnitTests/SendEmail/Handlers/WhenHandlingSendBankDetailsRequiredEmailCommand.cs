using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SendEmail.Handlers
{
    [TestFixture]
    public class WhenHandlingSendBankDetailsRequiredEmailCommand
    {
        private Mock<IMessageSession> _messageSession;
        private Mock<ILogger<SendBankDetailsRequiredEmailCommandHandler>> _logger;
        private Mock<IOptions<EmailTemplateSettings>> _settings;
        private SendBankDetailsRequiredEmailCommandHandler _sut;
        private EmailTemplateSettings _templates;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _messageSession = new Mock<IMessageSession>();
            _logger = new Mock<ILogger<SendBankDetailsRequiredEmailCommandHandler>>();
            _settings = new Mock<IOptions<EmailTemplateSettings>>();
            _templates = new EmailTemplateSettings { BankDetailsRequired = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() } };
            _settings.Setup(x => x.Value).Returns(_templates);
            _sut = new SendBankDetailsRequiredEmailCommandHandler(_messageSession.Object, _settings.Object, _logger.Object);
        }

        [Test]
        public async Task Then_the_send_email_command_is_published_to_the_message_bus()
        {
            // Arrange
            var command = _fixture.Create<SendBankDetailsRequiredEmailCommand>();
  
            // Act
            await _sut.Handle(command, new CancellationToken());

            // Assert
            _messageSession.Verify(x => x.Send(It.Is<SendEmailCommand>(x => x.RecipientsAddress == command.EmailAddress &&
                                                                       x.TemplateId == _templates.BankDetailsRequired.TemplateId &&
                                                                       x.Tokens.ContainsKey("bank details url")), It.IsAny<SendOptions>()), Times.Once());
        }
    }
}
