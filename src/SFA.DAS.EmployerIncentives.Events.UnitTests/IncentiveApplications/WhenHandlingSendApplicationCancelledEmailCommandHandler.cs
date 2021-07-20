﻿using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.IncentiveApplications
{
    [TestFixture]
    public class WhenHandlingSendApplicationCancelledEmailCommandHandler
    {
        private Mock<ICommandPublisher> _commandPublisher;
        private Mock<IHashingService> _hashingService;
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
            _hashingService = new Mock<IHashingService>();
        }

        [Test]
        public async Task Then_the_send_email_command_is_published_to_the_message_bus()
        {
            // Arrange
            var command = _fixture.Create<EmployerWithdrawn>();

            var hashedAccountId = _fixture.Create<string>();
            _hashingService.Setup(r => r.HashValue(It.Is<long>(x => x == command.AccountId))).Returns(hashedAccountId);

            var hashedAccountLegalEntityId = _fixture.Create<string>();
            _hashingService.Setup(r => r.HashValue(It.Is<long>(x => x == command.AccountLegalEntityId)))
                .Returns(hashedAccountLegalEntityId);

            const string baseUrl = "https://test.com";
            _configuration = new ApplicationSettings { EmployerIncentivesWebBaseUrl = baseUrl };
            _applicationSettings.Setup(x => x.Value).Returns(_configuration);


            var expectedUrl = $"{baseUrl}/{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications";

            _sut = new EmployerWithdrawnNotificationHandler(_commandPublisher.Object, _emailSettings.Object,
                _hashingService.Object, _applicationSettings.Object);

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
    }
}