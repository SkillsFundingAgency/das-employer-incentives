using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.Notifications.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class SendMetricsReportEmailCommandHandler : ICommandHandler<SendMetricsReportEmailCommand>
    {
        private readonly IReportsDataRepository _reportsDataRepository;
        private readonly IReportsRepository _reportsRepository;
        private readonly IConfiguration _configuration;
        private readonly ICommandPublisher _commandPublisher;
        private readonly EmailTemplateSettings _emailTemplates;
        
        private const string CollectionPeriodToken = "periodName";
        private const string AcademicYearToken = "academicYear";
        private const string AmountToBePaidToken = "amountToBePaid";
        private const string AmountFailingValidationToken = "amountFailingValidation";
        private const string MetricsReportDownloadToken = "metricsReportDownloadLink";


        public SendMetricsReportEmailCommandHandler(IReportsDataRepository reportsDataRepository,
                                                    IReportsRepository reportsRepository,
                                                    ICommandPublisher commandPublisher,
                                                    IOptions<EmailTemplateSettings> emailTemplates,
                                                    IConfiguration configuration)
        {
            _reportsDataRepository = reportsDataRepository;
            _reportsRepository = reportsRepository;
            _commandPublisher = commandPublisher;
            _emailTemplates = emailTemplates.Value;
            _configuration = configuration;
        }

        public async Task Handle(SendMetricsReportEmailCommand command, CancellationToken cancellationToken = default)
        {
            var report = await _reportsDataRepository.Execute<MetricsReport>();

            if (command.CollectionPeriod.PeriodNumber != report.CollectionPeriod.Period ||
                command.CollectionPeriod.AcademicYear.ToString() != report.CollectionPeriod.AcademicYear)
            {
                throw new ArgumentException($"Metrics report AY:{command.CollectionPeriod.PeriodNumber} Period {command.CollectionPeriod.PeriodNumber} does not match command AY:{report.CollectionPeriod.AcademicYear} Period:{report.CollectionPeriod.Period}");
            }

            var templateId = _emailTemplates.MetricsReport.TemplateId;

            var amountToBePaidInMillions = Math.Round(report.ValidationSummary.ValidRecords.PeriodAmount / 1000000, 2);
            var amountFailingValidationInMillions = Math.Round(report.ValidationSummary.InvalidRecords.PeriodAmount / 1000000, 2);

            var personalisationTokens = new Dictionary<string, string>
            {
                { CollectionPeriodToken, $"R{command.CollectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}" },
                { AcademicYearToken, command.CollectionPeriod.AcademicYear.ToString() },
                { AmountToBePaidToken, amountToBePaidInMillions.ToString()},
                { AmountFailingValidationToken, amountFailingValidationInMillions.ToString() }
            };

            var reportFileInfo = new ReportsFileInfo($"{_configuration["EnvironmentName"]} {report.Name} R{command.CollectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}_{command.CollectionPeriod.AcademicYear}",
                                                     "xlsx",
                                                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                                     "Metrics");

            var reportBytes = await _reportsRepository.Get(reportFileInfo);

            var attachments = new Dictionary<string, byte[]>{{ MetricsReportDownloadToken, reportBytes }};
            
            var sendEmailCommand = new SendEmailWithAttachmentsCommand(templateId, command.EmailAddress, personalisationTokens, attachments);

            await _commandPublisher.Publish(sendEmailCommand);
        }
    }
}