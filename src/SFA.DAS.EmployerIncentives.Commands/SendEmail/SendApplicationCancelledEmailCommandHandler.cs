using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendApplicationCancelledEmailCommandHandler : ICommandHandler<EmployerWithdrawalCommand>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly EmailTemplateSettings _emailTemplates;
        private readonly IAccountDomainRepository _accountDomainRepository;
        private readonly IHashingService _hashingService;
        private readonly ApplicationSettings _applicationSettings;

        private const string ViewApplicationsUrlToken = "view applications url";
        private const string OrganisationNameToken = "organisation name";
        public const string Uln = "uln";

        public SendApplicationCancelledEmailCommandHandler(ICommandPublisher commandPublisher,
            IOptions<EmailTemplateSettings> emailTemplates,
            IAccountDomainRepository accountDomainRepository,
            IHashingService hashingService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
            _accountDomainRepository = accountDomainRepository;
            _hashingService = hashingService;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(EmployerWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var template = _emailTemplates.ApplicationCancelled;
            var account = await _accountDomainRepository.Find(command.AccountId);
            var legalEntity = account?.GetLegalEntity(command.AccountLegalEntityId);

            if (legalEntity == null)
            {
                throw new ArgumentException($"Account legal entity not found - account id {command.AccountId} account legal entity id {command.AccountLegalEntityId}");
            }

            var personalisationTokens = new Dictionary<string, string>
            {
                {ViewApplicationsUrlToken, GenerateViewApplicationsUrl(command).AbsoluteUri},
                {OrganisationNameToken, legalEntity.Name},
                {Uln, command.ULN.ToString(CultureInfo.InvariantCulture)}
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            await _commandPublisher.Publish(sendEmailCommand, cancellationToken);
        }

        private Uri GenerateViewApplicationsUrl(EmployerWithdrawalCommand command)
        {
            var hashedAccountId = _hashingService.HashValue(command.AccountId);
            var hashedAccountLegalEntityId = _hashingService.HashValue(command.AccountLegalEntityId);
            var host = _applicationSettings.EmployerIncentivesWebBaseUrl;
            var url = new Uri(new Uri(host), $"{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications");

            return url;
        }
    }
}
