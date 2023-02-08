using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.IncentiveApplications
{
    [TestFixture]
    public class WhenHandlingSendApplicationCancelledEmailCommandHandler
    {
        private Mock<ICommandPublisher> _commandPublisher;
        private Mock<IEncodingService> _encodingService;
        private Mock<IOptions<EmailTemplateSettings>> _emailSettings;
        private Mock<IOptions<ApplicationSettings>> _applicationSettings;
        private ApplicationSettings _configuration;
        private EmployerWithdrawnNotificationHandler _sut;
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
            _applicationSettings = new Mock<IOptions<ApplicationSettings>>();
            _encodingService = new Mock<IEncodingService>();
        }

        [Test]
        public async Task Then_the_send_email_command_is_published_to_the_message_bus()
        {
            // Arrange
            var command = _fixture.Create<EmployerWithdrawn>();

            var hashedAccountId = _fixture.Create<string>();
            _encodingService.Setup(r => r.Encode(It.Is<long>(x => x == command.AccountId), EncodingType.AccountId)).Returns(hashedAccountId);

            var hashedAccountLegalEntityId = _fixture.Create<string>();
            _encodingService.Setup(r => r.Encode(It.Is<long>(x => x == command.AccountLegalEntityId), EncodingType.AccountId))
                .Returns(hashedAccountLegalEntityId);

            const string baseUrl = "https://test.com";
            _configuration = new ApplicationSettings { EmployerIncentivesWebBaseUrl = baseUrl };
            _applicationSettings.Setup(x => x.Value).Returns(_configuration);

            var expectedUrl = $"{baseUrl}/{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications";

            _sut = new EmployerWithdrawnNotificationHandler(_commandPublisher.Object, _emailSettings.Object, _encodingService.Object, _applicationSettings.Object);

            // Act
            await _sut.Handle(command);

            // Assert
            _commandPublisher.Verify(cp => cp.Publish(It.Is<SendEmailCommand>(x =>
                x.RecipientsAddress == command.EmailAddress &&
                x.TemplateId == _templates.ApplicationCancelled.TemplateId &&
                x.Tokens["view applications url"] == expectedUrl &&
                x.Tokens["organisation name"] == command.LegalEntity &&
                x.Tokens["uln"] == command.Model.ULN.ToString()
            ), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Then_an_email_is_not_sent_when_no_email_address_is_provided()
        {

            // Arrange
            var command = new EmployerWithdrawn(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<string>(), (string)null, _fixture.Create<ApprenticeshipModel>(), _fixture.Create<ServiceRequest>());

            _sut = new EmployerWithdrawnNotificationHandler(_commandPublisher.Object, _emailSettings.Object, _encodingService.Object, _applicationSettings.Object);

            // Act
            await _sut.Handle(command);

            // Assert
            _commandPublisher.Verify(cp => cp.Publish(It.IsAny<SendEmailCommand>(), It.IsAny<CancellationToken>()), Times.Never());
        }
    }
}
