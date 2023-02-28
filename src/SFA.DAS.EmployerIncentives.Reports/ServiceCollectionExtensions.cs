using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Data.Reports;
using SFA.DAS.EmployerIncentives.Data.Reports.Metrics;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
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

            serviceCollection.AddReportsConnection();

            return serviceCollection;
        }

        private static IServiceCollection AddReportsConnection(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IReportsConnectionProvider>(s =>
            {
                var settings = s.GetService<IOptions<ApplicationSettings>>().Value;

                return new ReportsConnectionProvider(settings.DbConnectionString);
            });

            return serviceCollection;
        }
    }
}
