using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsRequiredEmailCommandHandler : ICommandHandler<SendBankDetailsRequiredEmailCommand>
    {
        private readonly IMessageSession _messageSession;
        private readonly EmailTemplateSettings _emailTemplates;
        private readonly ILogger<SendBankDetailsRequiredEmailCommandHandler> _logger;
        private const string AddBankDetailsUrlToken = "bank details url";

        public SendBankDetailsRequiredEmailCommandHandler(IMessageSession messageSession, IOptions<EmailTemplateSettings> emailTemplates, 
                                                  ILogger<SendBankDetailsRequiredEmailCommandHandler> logger)
        {
            _messageSession = messageSession;
            _emailTemplates = emailTemplates.Value;
            _logger = logger;
        }

        public async Task Handle(SendBankDetailsRequiredEmailCommand command, CancellationToken cancellationToken = default)
        {
            var template = _emailTemplates.BankDetailsRequired;

            var personalisationTokens = new Dictionary<string, string>
            {
                { AddBankDetailsUrlToken, command.AddBankDetailsUrl }
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            try
            {
                _logger.LogInformation($"Sending bank details required email for account id {command.AccountId} legal entity id {command.AccountLegalEntityId}");
                await _messageSession.Send(sendEmailCommand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending bank details required email to account id {command.AccountId} legal entity id {command.AccountLegalEntityId}");
            }
        }
    }
}
