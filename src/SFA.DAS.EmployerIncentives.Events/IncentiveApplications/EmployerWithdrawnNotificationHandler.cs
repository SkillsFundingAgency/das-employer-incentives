﻿using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class EmployerWithdrawnNotificationHandler :  IDomainEventHandler<EmployerWithdrawn>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IHashingService _hashingService;
        private readonly EmailTemplateSettings _emailTemplates;
        private readonly ApplicationSettings _applicationSettings;

        private const string ViewApplicationsUrlToken = "view applications url";
        private const string OrganisationNameToken = "organisation name";
        private const string Uln = "uln";

        public EmployerWithdrawnNotificationHandler(
            ICommandPublisher commandPublisher,
            IOptions<EmailTemplateSettings> emailTemplates,
            IHashingService hashingService,
            IOptions<ApplicationSettings> applicationSettings)
        {
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
            _hashingService = hashingService;
            _applicationSettings = applicationSettings.Value;
        }

        public async Task Handle(EmployerWithdrawn command, CancellationToken cancellationToken = default)
        {
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
            var hashedAccountId = _hashingService.HashValue(command.AccountId);
            var hashedAccountLegalEntityId = _hashingService.HashValue(command.AccountLegalEntityId);
            var host = _applicationSettings.EmployerIncentivesWebBaseUrl;
            var url = new Uri(new Uri(host),
                $"{hashedAccountId}/payments/{hashedAccountLegalEntityId}/payment-applications");

            return url;
        }
    }
}