using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using System;
using System.IO;
using System.Reflection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, IConfiguration configuration)
        {

            var env = Environment.GetEnvironmentVariable("EnvironmentName");
            var configFileName = "nlog.config";
            if (string.IsNullOrEmpty(env) || env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                configFileName = "nlog.local.config";
            }
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            var configFilePath = Directory.GetFiles(rootDirectory, configFileName, SearchOption.AllDirectories)[0];
            LogManager.Setup()
                .SetupExtensions(e => e.AutoLoadAssemblies(false))
                .LoadConfigurationFromFile(configFilePath, optional: false)
                .LoadConfiguration(builder => builder.LogFactory.AutoShutdown = false)
                .GetCurrentClassLogger();

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Debug); // this is because all logging is filtered out by default
                options.SetMinimumLevel(LogLevel.Trace);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();
            });

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            UpdateableServiceProvider container = null;

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() =>  container.GetService<ISqlConnectionProvider>().Get(configuration["ApplicationSettings:DbConnectionString"]))
                .UseUnitOfWork();
            
            if (configuration["ApplicationSettings:NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(configuration.GetValue("ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport")));
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration["ApplicationSettings:NServiceBusConnectionString"], r => r.AddRouting());
            }

            if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
            {
                endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
            }

            var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
            container = new UpdateableServiceProvider(serviceCollection);
            endpointWithExternallyManagedServiceProvider.Start(container);
            serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

            return serviceCollection;
        }
    }
}
