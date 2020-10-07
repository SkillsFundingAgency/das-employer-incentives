using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsRequiredEmailCommandHandler : ICommandHandler<SendBankDetailsRequiredEmailCommand>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly EmailTemplateSettings _emailTemplates;
        private const string AddBankDetailsUrlToken = "bank details url";

        public SendBankDetailsRequiredEmailCommandHandler(ICommandPublisher commandPublisher, 
                                                          IOptions<EmailTemplateSettings> emailTemplates)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
        }

        public async Task Handle(SendBankDetailsRequiredEmailCommand command, CancellationToken cancellationToken = default)
        {
            var template = _emailTemplates.BankDetailsRequired;

            var personalisationTokens = new Dictionary<string, string>
            {
                { AddBankDetailsUrlToken, command.AddBankDetailsUrl }
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            await _commandPublisher.Publish(sendEmailCommand);           
        }
    }
}
