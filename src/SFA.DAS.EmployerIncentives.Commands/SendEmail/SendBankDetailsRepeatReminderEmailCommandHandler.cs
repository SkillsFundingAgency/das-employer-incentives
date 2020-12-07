using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsRepeatReminderEmailCommandHandler : ICommandHandler<SendBankDetailsRepeatReminderEmailCommand>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly EmailTemplateSettings _emailTemplates;
        private readonly IAccountDomainRepository _accountDomainRepository;
        private const string AddBankDetailsUrlToken = "bank details url";
        private const string OrganisationNameToken = "organisation name";

        public SendBankDetailsRepeatReminderEmailCommandHandler(ICommandPublisher commandPublisher,
                                                               IOptions<EmailTemplateSettings> emailTemplates,
                                                               IAccountDomainRepository accountDomainRepository)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
            _accountDomainRepository = accountDomainRepository;
        }

        public async Task Handle(SendBankDetailsRepeatReminderEmailCommand command, CancellationToken cancellationToken = default)
        {
            var template = _emailTemplates.BankDetailsRepeatReminder;
            var account = await _accountDomainRepository.Find(command.AccountId);
            var legalEntity = account?.GetLegalEntity(command.AccountLegalEntityId);
            if (legalEntity == null)
            {
                throw new ArgumentException($"Account legal entity not found - account id {command.AccountId} account legal entity id {command.AccountLegalEntityId}");
            }

            var personalisationTokens = new Dictionary<string, string>
            {
                { AddBankDetailsUrlToken, command.AddBankDetailsUrl },
                { OrganisationNameToken, legalEntity.Name }
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            await _commandPublisher.Publish(sendEmailCommand);
        }
    }
}
