using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.SqlServer.DependencyResolution.Microsoft;
using System;
using System.Data.Common;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers.Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {            
            builder.Services.AddLogging(logBuilder =>
            {
                logBuilder.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by defualt
                var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
                logBuilder.AddNLog(Directory.GetFiles(rootDirectory, "nlog.config", SearchOption.AllDirectories)[0]);
            });

            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(config)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsLocalOrAcceptanceTests(config))
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = config["ConfigNames"].Split(",");
                    options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = config["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }
#if DEBUG
            if (!config["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                configBuilder.AddJsonFile($"local.settings.json", optional: true);
            }
#endif
            config = configBuilder.Build();

            builder.Services.AddOptions();            
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));

            builder.Services.AddUnitOfWork();

            // Required for the sql unit of work
            builder.Services.AddScoped<DbConnection>(p =>
            {
                var settings = p.GetService<IOptions<ApplicationSettings>>();
                return new SqlConnection(settings.Value.DbConnectionString);
            });

            builder.Services.AddNServiceBus(config);
            builder.Services.AddEntityFrameworkForEmployerIncentives()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddSqlServerUnitOfWork();

            builder.Services.AddPersistenceServices();
            builder.Services.AddQueryServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();

            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            if (!ConfigurationIsLocalOrAcceptanceTests(config))
            { 
                builder.Services.AddNServiceBus(logger);
            }
            else if(ConfigurationIsLocalOrDev(config))
            {
                builder.Services.AddNServiceBus(
                    logger,
                    (options) =>
                    {
                        if (config["ApplicationSettings:NServiceBusConnectionString"] == "UseLearningEndpoint=true")
                        {
                            options.EndpointConfiguration = (endpoint) =>
                            {
                                endpoint.UseTransport<LearningTransport>().StorageDirectory(config.GetValue("ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport")));
                                Commands.Types.RoutingSettingsExtensions.AddRouting(endpoint.UseTransport<LearningTransport>().Routing());
                                return endpoint;
                            };
                        }
                    });
            }
        }

        private bool ConfigurationIsLocalOrAcceptanceTests(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }

        private bool ConfigurationIsLocalOrDevOrAcceptanceTests(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }

        private bool ConfigurationIsLocalOrDev(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}