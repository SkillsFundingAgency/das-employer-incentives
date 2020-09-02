using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.UnitOfWork;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers.Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
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

            if (!configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
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
            configBuilder.AddJsonFile($"local.settings.json", optional: true);
#endif
            var config = configBuilder.Build();

            builder.Services.AddOptions();
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.Configure<PolicySettings>(config.GetSection("PolicySettings"));

            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);
            if (config["ApplicationSettings:NServiceBusConnectionString"] == "UseLearningEndpoint=true")
            {
                builder.Services.AddNServiceBus(logger, (options) =>
                {
                    options.EndpointConfiguration = (endpoint) =>
                    {
                        endpoint.UseTransport<LearningTransport>().StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport"));
                        return endpoint;
                    };
                });
            }
            else
            {
                builder.Services.AddNServiceBus(logger);
            }

            builder.Services.AddPersistenceServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();

            builder.Services.AddEntityFrameworkForEmployerIncentives()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddNServiceBusClientUnitOfWork();

            builder.Services.AddTransient<UnitOfWorkManagerMiddleware>();
        }
    }
}