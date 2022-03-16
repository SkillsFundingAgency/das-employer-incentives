using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class SendMetricsReportCommandHandler : ICommandHandler<SendMetricsReportCommand>
    {
        private readonly IExcelReports _reports;

        public SendMetricsReportCommandHandler(IExcelReports reports)
        {
            _reports = reports;
        }

        public async Task Handle(SendMetricsReportCommand command, CancellationToken cancellationToken = default)
        {
            var report = await _reports.GenerateMetricsReport();
            report.SetName($"{DateTime.Today:yyMMdd}_{report.Name}");
            await _reports.Save(report);
        }
    }
}
