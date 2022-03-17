using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Excel
{
    public interface IExcelReports
    {
        Task<MetricsReport> GenerateMetricsReport();
        Task Save(MetricsReport report);
    }
}
