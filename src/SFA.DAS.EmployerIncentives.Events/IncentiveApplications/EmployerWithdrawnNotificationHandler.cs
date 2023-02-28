using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class EmployerWithdrawnNotificationHandler :  IDomainEventHandler<EmployerWithdrawn>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IEncodingService _encodingService;
        private readonly EmailTemplateSettings _emailTemplates;
        private readonly ApplicationSettings _applicationSettings;

        private const string ViewApplicationsUrlToken = "view applications url";
        private const string OrganisationNameToken = "organisation name";
        private const string Uln = "uln";

        public EmployerWithdrawnNotificationHandler(
            ICommandPublisher commandPublisher,
            IOptions<EmailTemplateSettings> emailTemplates,
            IEncodingService encodingService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
            _encodingService = encodingService;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(EmployerWithdrawn command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(command.EmailAddress))
            {
                return;
            }

            var template = _emailTemplates.ApplicationCancelled;
            var personalisationTokens = new Dictionary<string, string>
            {
                {ViewApplicationsUrlToken, GenerateViewApplicationsUrl(command).AbsoluteUri},
                {OrganisationNameToken,command.LegalEntity},
                {Uln, command.Model.ULN.ToString(CultureInfo.InvariantCulture)}
            };

            var sendEmailCommand = new SendEmailCommand(template.TemplateId, command.EmailAddress, personalisationTokens);

            await _commandPublisher.Publish(sendEmailCommand, cancellationToken);
        }

        private Uri GenerateViewApplicationsUrl(EmployerWithdrawn command)
        {
            var hashedAccountId = _encodingService.Encode(command.AccountId, EncodingType.AccountId);
            var hashedAccountLegalEntityId = _encodingService.Encode(command.AccountLegalEntityId, EncodingType.AccountId);
            var host = _applicationSettings.EmployerIncentivesWebBaseUrl;
            var url = new Uri(new Uri(host),
                $"{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications");

            return url;
        }
    }
}
