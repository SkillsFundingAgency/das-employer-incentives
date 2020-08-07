using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsEmailCommandHandler : ICommandHandler<SendBankDetailsEmailCommand>
    {
        private readonly INotificationsApi _notificationsApi;
        private readonly NotifyServiceSettings _configuration;
        private readonly ILogger<SendBankDetailsEmailCommandHandler> _logger;
        private const string TemplateName = "BankDetailsRequired";
        private const string AddBankDetailsUrlToken = "bank details url";

        public SendBankDetailsEmailCommandHandler(INotificationsApi notificationsApi, IOptions<NotifyServiceSettings> configuration, 
                                                  ILogger<SendBankDetailsEmailCommandHandler> logger)
        {
            _notificationsApi = notificationsApi;
            _configuration = configuration.Value;
            _logger = logger;
        }

        public async Task Handle(SendBankDetailsEmailCommand command, CancellationToken cancellationToken = default)
        {
            var template = _configuration.EmailTemplates.FirstOrDefault(x => x.Name == TemplateName);
            if (template == null)
            {
                _logger.LogError($"Email template id {TemplateName} not found in configuration)");
                return;
            }

            var personalisationTokens = new Dictionary<string, string>
            {
                { AddBankDetailsUrlToken, command.AddBankDetailsUrl }
            };

            var email = new Email
            {
                RecipientsAddress = command.EmailAddress,
                TemplateId = template.TemplateId,
                ReplyToAddress = template.ReplyToAddress,
                SystemId = _configuration.SystemId,
                Subject = template.Subject,
                Tokens = personalisationTokens
            };

            try
            {
                _logger.LogInformation($"Sending bank details required email for account id {command.AccountId} legal entity id {command.AccountLegalEntityId}");
                await _notificationsApi.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending bank details required email to account id {command.AccountId} legal entity id {command.AccountLegalEntityId}");
            }
        }
    }
}
