using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var env = Environment.GetEnvironmentVariable("EnvironmentName");
            var configFileName = "nlog.config";
            if (string.IsNullOrEmpty(env) || env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) || env.Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                configFileName = "nlog.local.config";
            }
            var rootDirectory = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ".."));
            var configFilePath = Directory.GetFiles(rootDirectory, configFileName, SearchOption.AllDirectories)[0];
            LogManager.Setup()
                .SetupExtensions(e => e.AutoLoadExtensions())
                .LoadConfigurationFromFile(configFilePath, optional: false)
                .LoadConfiguration(builder => builder.LogFactory.AutoShutdown = false)
                .GetCurrentClassLogger();

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by default
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
#if DEBUG
                options.AddConsole();
#endif
            });

            return serviceCollection;
        }
        //public static void StartNServiceBus(this UpdateableServiceProvider serviceProvider, 
        //    IConfiguration configuration)
        //{
        //    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
        //        .UseMessageConventions()
        //        .UseNewtonsoftJsonSerializer()
        //        .UseOutbox(true)
        //        .UseSqlServerPersistence(() => new SqlConnection(configuration["ApplicationSettings:DbConnectionString"]))
        //        .UseUnitOfWork();

        //    if (configuration["ApplicationSettings:NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        endpointConfiguration
        //            .UseTransport<LearningTransport>()
        //            .StorageDirectory(configuration.GetValue("ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src")], @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport")));
        //        endpointConfiguration.UseLearningTransport(s => s.AddRouting());
        //    }
        //    else
        //    {
        //        endpointConfiguration
        //            .UseAzureServiceBusTransport(configuration["ApplicationSettings:NServiceBusConnectionString"], r => r.AddRouting());
        //    }

        //    if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
        //    {
        //        endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
        //    }

        //    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        //    serviceProvider.AddSingleton(p => endpoint)
        //        .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
        //        .AddHostedService<NServiceBusHostedService>();
        //}

        //public static IServiceCollection AddNServiceBus(
        //    this IServiceCollection serviceCollection,
        //    IConfiguration configuration)
        //{
        //    var webBuilder = serviceCollection.AddWebJobs(x => { });
        //    webBuilder.AddExecutionContextBinding();

        //    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
        //        .UseMessageConventions()
        //        .UseNewtonsoftJsonSerializer()
        //        .UseOutbox(true)
        //        .UseSqlServerPersistence(() => new SqlConnection(configuration["ApplicationSettings:DbConnectionString"]))
        //        .UseUnitOfWork();
            
        //    if (configuration["ApplicationSettings:NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        endpointConfiguration
        //            .UseTransport<LearningTransport>()
        //            .StorageDirectory(configuration.GetValue("ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src")], @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport")));
        //        endpointConfiguration.UseLearningTransport(s => s.AddRouting());
        //    }
        //    else
        //    {
        //        endpointConfiguration
        //            .UseAzureServiceBusTransport(configuration["ApplicationSettings:NServiceBusConnectionString"], r => r.AddRouting());
        //    }

        //    if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
        //    {
        //        endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
        //    }

        //    var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);
        //    var endPointInstance = endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection)).GetAwaiter().GetResult();
        //    serviceCollection.AddSingleton(endPointInstance);
        //    serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

        //    return serviceCollection;

        //    //var endpointWithExternallyManagedServiceProvider = EndpointWithExternallyManagedServiceProvider.Create(endpointConfiguration, serviceCollection);

        //    ////var endPointInstance = endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection)).GetAwaiter().GetResult();
        //    //var endPointInstance = endpointWithExternallyManagedServiceProvider.Start(new UpdateableServiceProvider(serviceCollection);

        //    //serviceCollection.AddSingleton<IEndpointInstance>(s =>  );

        //    //serviceCollection.AddSingleton(endPointInstance);
        //    ////serviceCollection.AddSingleton(typeof(IServiceProviderFactory<IServiceCollection>), new UpdateableServiceProvider(serviceCollection));

        //    //serviceCollection.AddSingleton(p => endPointInstance)
        //    //    //.AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
        //    //    .AddSingleton(endpointWithExternallyManagedServiceProvider.MessageSession.Value);

        //    ////..    .AddHostedService<NServiceBusHostedService>();

        //    ////serviceCollection.AddSingleton(p => endpointWithExternallyManagedServiceProvider.MessageSession.Value);

        //    //return serviceCollection;
        //}
    }
}
