using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging((options) =>
            {
                options.SetMinimumLevel(LogLevel.Trace);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog();
            });

            return serviceCollection;
        }

        public static IServiceCollection AddEntityFrameworkForEmployerIncentives(this IServiceCollection services)
        {
            return services.AddScoped(p =>
            {
                var settings = p.GetService<IOptions<ApplicationSettings>>();
                var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseSqlServer(settings.Value.DbConnectionString);

                var dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options);

                return dbContext;
            });
        }
    }
}
