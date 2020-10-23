using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddNLog();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }
#if DEBUG
            if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                configBuilder.AddJsonFile($"local.settings.json", optional: true);
            }
#endif
            var config = configBuilder.Build();

            builder.Services.AddOptions();
            builder.Services.AddUnitOfWork();
            builder.Services.AddEntityFrameworkForEmployerIncentives().AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>();

            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.Configure<ApplicationSettings>(config.GetSection("PolicySettings"));

            builder.Services.AddPersistenceServices();
            builder.Services.AddQueryServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();
        }

        private bool ConfigurationIsLocalOrAcceptanceTests(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}