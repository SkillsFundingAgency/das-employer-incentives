using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.IO;
using System.Reflection;
using NServiceBus;

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
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsLocalOrAcceptanceTests(configuration))
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
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));

            builder.Services.AddCommandService();
            
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