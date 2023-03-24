using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.Notifications.Messages.Commands;
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
        private const string MetricsReportDownloadToken = "metricsReport";


        public SendMetricsReportEmailCommandHandler(ICommandPublisher commandPublisher,
                                                    IOptions<EmailTemplateSettings> emailTemplates,
                                                    IOptions<ApplicationSettings> options,
                                                    IReportsDataRepository reportsDataRepository,
                                                    IReportsRepository reportsRepository,
                                                    IConfiguration configuration)
        {
            _emailTemplates = emailTemplates.Value;
            _commandPublisher = commandPublisher;
            _reportsDataRepository = reportsDataRepository;
            _reportsRepository = reportsRepository;
            _configuration = configuration;
        }

        public async Task Handle(SendMetricsReportEmailCommand command, CancellationToken cancellationToken = default)
        {
            var report = await _reportsDataRepository.Execute<MetricsReport>();

            var templateId = _emailTemplates.MetricsReport.TemplateId;

            var personalisationTokens = new Dictionary<string, string>
            {
                { CollectionPeriodToken, command.CollectionPeriod.PeriodNumber.ToString() },
                { AcademicYearToken, command.CollectionPeriod.AcademicYear.ToString() },
                { AmountToBePaidToken, report.ValidationSummary.ValidRecords.PeriodAmount.ToString()},
                { AmountFailingValidationToken, report.ValidationSummary.InvalidRecords.PeriodAmount.ToString() }
            };

            var reportFileInfo = new ReportsFileInfo($"{_configuration["EnvironmentName"]} {report.Name} R{command.CollectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}_{command.CollectionPeriod.AcademicYear}",
                                                     "xlsx",
                                                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                                     "Metrics");

            var reportBytes = await _reportsRepository.Get(reportFileInfo);

            var attachments = new DataBusProperty<Dictionary<string,byte[]>>(new Dictionary<string, byte[]>
            {
                { MetricsReportDownloadToken, reportBytes },
            });
            
            var sendEmailCommand = new SendEmailWithAttachmentsCommand(templateId, command.EmailAddress, personalisationTokens, attachments);

            await _commandPublisher.Publish(sendEmailCommand);
        }
    }
}