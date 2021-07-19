using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
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
    public class WhenHandlingSendApplicationCancelledEmailCommandHandler
    {
        private Mock<ICommandPublisher> _commandPublisher;
        private Mock<IAccountDomainRepository> _accountRepository;
        private Mock<IHashingService> _hashingService;
        private Mock<IOptions<EmailTemplateSettings>> _emailSettings;
        private Mock<IOptions<ApplicationSettings>> _applicationSettings;
        private ApplicationSettings _configuration;
        private SendApplicationCancelledEmailCommandHandler _sut;
        private EmailTemplateSettings _templates;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _commandPublisher = new Mock<ICommandPublisher>();
            _emailSettings = new Mock<IOptions<EmailTemplateSettings>>();
            _templates = new EmailTemplateSettings { ApplicationCancelled = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() } };
            _emailSettings.Setup(x => x.Value).Returns(_templates);
            _accountRepository = new Mock<IAccountDomainRepository>();
            _applicationSettings = new Mock<IOptions<ApplicationSettings>>();
            _hashingService = new Mock<IHashingService>();
        }

        [Test]
        public async Task Then_the_send_email_command_is_published_to_the_message_bus()
        {
            // Arrange
            var command = _fixture.Create<EmployerWithdrawalCommand>();

            var account = Account.New(command.AccountId);
            var legalEntity = _fixture.Create<LegalEntity>();
            account.AddLegalEntity(command.AccountLegalEntityId, legalEntity);
            _accountRepository.Setup(r => r.Find(It.Is<long>(x => x == command.AccountId))).ReturnsAsync(account);

            var hashedAccountId = _fixture.Create<string>();
            _hashingService.Setup(r => r.HashValue(It.Is<long>(x => x == command.AccountId))).Returns(hashedAccountId);

            var hashedAccountLegalEntityId = _fixture.Create<string>();
            _hashingService.Setup(r => r.HashValue(It.Is<long>(x => x == command.AccountLegalEntityId)))
                .Returns(hashedAccountLegalEntityId);

            const string baseUrl = "https://test.com";
            _configuration = new ApplicationSettings {EmployerIncentivesWebBaseUrl = baseUrl};
            _applicationSettings.Setup(x => x.Value).Returns(_configuration);
            _sut = new SendApplicationCancelledEmailCommandHandler(_commandPublisher.Object, _emailSettings.Object,
                _accountRepository.Object,
                _hashingService.Object, _applicationSettings.Object);

            var expectedUrl = $"{baseUrl}/{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications";
          
            // Act
            await _sut.Handle(command);

            // Assert
            _commandPublisher.Verify(cp => cp.Publish(It.Is<SendEmailCommand>(x =>
                x.RecipientsAddress == command.EmailAddress &&
                x.TemplateId == _templates.ApplicationCancelled.TemplateId &&
                x.Tokens["view applications url"] == expectedUrl &&
                x.Tokens["organisation name"] == legalEntity.Name &&
                x.Tokens["uln"] == command.ULN.ToString()
            ), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public void Then_the_send_email_command_throws_exception_when_account_legal_entity_is_not_found()
        {
            // Arrange
            var command = _fixture.Create<EmployerWithdrawalCommand>();

            var account = Account.New(command.AccountId);
            _accountRepository.Setup(x => x.Find(It.Is<long>(x => x == command.AccountId))).ReturnsAsync(account);
         
            _sut = new SendApplicationCancelledEmailCommandHandler(_commandPublisher.Object, _emailSettings.Object,
                _accountRepository.Object,
                _hashingService.Object, _applicationSettings.Object);

            // Act
            Func<Task> action = async () => await _sut.Handle(command);

            // Assert
            action.Should().Throw<ArgumentException>();
            _commandPublisher.Verify(cp => cp.Publish(
                It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
