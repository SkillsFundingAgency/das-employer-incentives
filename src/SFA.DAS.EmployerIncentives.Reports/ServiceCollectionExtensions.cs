using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Reports.Excel;
using SFA.DAS.EmployerIncentives.Reports.Respositories;

namespace SFA.DAS.EmployerIncentives.Reports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReportServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IReportsRepository, AzureBlobRepository>();
            serviceCollection.AddScoped<IExcelReports, ExcelReportsService>();
            serviceCollection.AddScoped<IReportsDataRepository, ReportsDataRepository>();

            return serviceCollection;
        }
    }
}
