using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Reports.Reports.Metrics;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Excel
{
    public class ExcelReportsService : IExcelReports
    {     
        private readonly IReportsRepository _reportsRepository;
        private readonly IReportsDataRepository _reportsDataRepository;

        public ExcelReportsService(
            IReportsRepository reportsRepository, 
            IReportsDataRepository reportsDataRepository)
        {
            _reportsRepository = reportsRepository;
            _reportsDataRepository = reportsDataRepository;
        }

        public Task<MetricsReport> GenerateMetricsReport()
        {
            return _reportsDataRepository.Execute();
        }

        public async Task Save(MetricsReport report)
        {
            using var ms = new MemoryStream();            
            var reportGenerator = new MetricsReportGenerator(report);
            reportGenerator.Create(ms);
            await _reportsRepository.Save(
                new ReportsFileInfo(report.Name, "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Metrics"),
                ms);
        }
    }
}
