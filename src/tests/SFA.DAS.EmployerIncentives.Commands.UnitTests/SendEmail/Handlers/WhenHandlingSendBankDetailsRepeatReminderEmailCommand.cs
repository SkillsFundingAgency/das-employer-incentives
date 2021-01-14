using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.SendEmail.Handlers
{
    [TestFixture]
    public class WhenHandlingSendBankDetailsRepeatReminderEmailCommand
    {
        private Mock<ICommandPublisher> _commandPublisher;
        private Mock<IAccountDomainRepository> _accountRepository;
        private Mock<IHashingService> _hashingService;
        private string _hashedAccountId;
        private Mock<IOptions<EmailTemplateSettings>> _emailSettings;
        private Mock<IOptions<ApplicationSettings>> _applicationSettings;
        private ApplicationSettings _configuration;
        private SendBankDetailsRepeatReminderEmailCommandHandler _sut;
        private EmailTemplateSettings _templates;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _commandPublisher = new Mock<ICommandPublisher>();
            _emailSettings = new Mock<IOptions<EmailTemplateSettings>>();
            _templates = new EmailTemplateSettings { BankDetailsRepeatReminder = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() } };
            _emailSettings.Setup(x => x.Value).Returns(_templates);
            _accountRepository = new Mock<IAccountDomainRepository>();
            _applicationSettings = new Mock<IOptions<ApplicationSettings>>();
            _hashingService = new Mock<IHashingService>();
        }

        [TestCase("https://test.com")]
        [TestCase("https://test.com/")]
        public async Task Then_the_send_email_command_is_published_to_the_message_bus(string baseUrl)
        {
            // Arrange
            var command = _fixture.Create<SendBankDetailsRepeatReminderEmailCommand>();

            var account = Account.New(command.AccountId);
            var legalEntity = _fixture.Create<LegalEntity>();
            account.AddLegalEntity(command.AccountLegalEntityId, legalEntity);
            _accountRepository.Setup(x => x.Find(It.Is<long>(x => x == command.AccountId))).ReturnsAsync(account);
            _hashedAccountId = _fixture.Create<string>();
            _hashingService.Setup(x => x.HashValue(It.Is<long>(x => x == command.AccountId))).Returns(_hashedAccountId);

            _configuration = new ApplicationSettings { EmployerIncentivesWebBaseUrl = baseUrl };
            _applicationSettings.Setup(x => x.Value).Returns(_configuration);
            _sut = new SendBankDetailsRepeatReminderEmailCommandHandler(_commandPublisher.Object, _emailSettings.Object, _accountRepository.Object,
                                                            _hashingService.Object, _applicationSettings.Object);

            var expectedUrl = $"{_hashedAccountId}/bank-details/{command.ApplicationId}/add-bank-details";
            if (baseUrl.EndsWith("/"))
            {
                expectedUrl = baseUrl + expectedUrl;
            }
            else
            {
                expectedUrl = baseUrl + "/" + expectedUrl;
            }

            // Act
            await _sut.Handle(command, new CancellationToken());

            // Assert
            _commandPublisher.Verify(x => x.Publish(It.Is<SendEmailCommand>(x => x.RecipientsAddress == command.EmailAddress &&
                                                                       x.TemplateId == _templates.BankDetailsRepeatReminder.TemplateId &&
                                                                       x.Tokens["bank details url"] == expectedUrl &&
                                                                       x.Tokens["organisation name"] == legalEntity.Name
                                                                       ), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
