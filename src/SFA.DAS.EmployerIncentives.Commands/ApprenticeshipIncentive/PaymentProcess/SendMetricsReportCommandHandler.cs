using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Reports;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class SendMetricsReportCommandHandler : ICommandHandler<SendMetricsReportCommand>
    {
        private readonly IReportsDataRepository _reportsDataRepository;
        private readonly IReportsRepository _reportsRepository;
        private readonly IExcelReportGenerator<MetricsReport> _metricsExcelReportGenerator;
        private readonly IConfiguration _configuration;

        public SendMetricsReportCommandHandler(
            IReportsDataRepository reportsDataRepository,
            IReportsRepository reportsRepository,
            IExcelReportGenerator<MetricsReport> metricsExcelReportGenerator,
            IConfiguration configuration)
        {
            _reportsDataRepository = reportsDataRepository;
            _reportsRepository = reportsRepository;
            _metricsExcelReportGenerator = metricsExcelReportGenerator;
            _configuration = configuration;
        }

        public async Task Handle(SendMetricsReportCommand command, CancellationToken cancellationToken = default)
        {
            var report = await _reportsDataRepository.Execute<MetricsReport>();
            
            await _reportsRepository.Save(
                new ReportsFileInfo($"{_configuration["EnvironmentName"]} {report.Name} R{command.CollectionPeriod.PeriodNumber.ToString().PadLeft(2, '0')}_{command.CollectionPeriod.AcademicYear}", "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics"), 
                _metricsExcelReportGenerator.Create(report));
        }
    }
}
