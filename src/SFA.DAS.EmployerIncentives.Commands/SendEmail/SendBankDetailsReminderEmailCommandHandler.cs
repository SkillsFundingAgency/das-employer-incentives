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
    public class SendBankDetailsReminderEmailCommandHandler : ICommandHandler<SendBankDetailsReminderEmailCommand>
    {
        private readonly ICommandPublisher<SendEmailCommand> _commandPublisher;
        private readonly EmailTemplateSettings _emailTemplates;
        private const string AddBankDetailsUrlToken = "bank details url";

        public SendBankDetailsReminderEmailCommandHandler(ICommandPublisher<SendEmailCommand> commandPublisher, 
                                                          IOptions<EmailTemplateSettings> emailTemplates)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
        }

        public async Task Handle(SendBankDetailsReminderEmailCommand command, CancellationToken cancellationToken = default)
        {
            var template = _emailTemplates.BankDetailsReminder;

            var personalisationTokens = new Dictionary<string, string>
            {
                { AddBankDetailsUrlToken, command.AddBankDetailsUrl }
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            await _commandPublisher.Publish(sendEmailCommand);           
        }
    }
}
