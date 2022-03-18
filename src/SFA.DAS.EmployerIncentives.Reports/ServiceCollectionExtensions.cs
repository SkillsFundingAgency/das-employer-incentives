using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using SFA.DAS.EmployerIncentives.Reports.Excel.Metrics;
using SFA.DAS.EmployerIncentives.Reports.Respositories;

namespace SFA.DAS.EmployerIncentives.Reports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IReportsRepository, AzureBlobRepository>();
            serviceCollection.AddScoped<IReportsDataRepository, ReportsDataRepository>();
            serviceCollection.AddScoped<IExcelReportGenerator<MetricsReport>, MetricsReportGenerator>();

            return serviceCollection;
        }
    }
}
