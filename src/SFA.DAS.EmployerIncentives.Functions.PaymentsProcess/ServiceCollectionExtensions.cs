using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NServiceBus;
using NServiceBus.Container;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by defualt
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

        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            Console.WriteLine("******************* Configuration variables *******************");
            Console.WriteLine("EnvironmentName" + configuration["EnvironmentName"]);
            Console.WriteLine("ApplicationSettings:DbConnectionString" + configuration["ApplicationSettings:DbConnectionString"]);
            Console.WriteLine("ApplicationSettings:NServiceBusConnectionString" + configuration["ApplicationSettings:NServiceBusConnectionString"]);
            Console.WriteLine("ApplicationSettings:UseLearningEndpointStorageDirectory" + configuration["ApplicationSettings:UseLearningEndpointStorageDirectory"]);
            Console.WriteLine("ApplicationSettings:NServiceBusLicense" + configuration["ApplicationSettings:NServiceBusLicense"]);

            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["ApplicationSettings:DbConnectionString"]))
                .UseUnitOfWork();

            endpointConfiguration.UseContainer<ServicesBuilder>((Action<ContainerCustomizations>)(c => c.ExistingServices(serviceCollection)));
            
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

            var endpoint = Endpoint.Start(endpointConfiguration);
            Task.WhenAll(endpoint);

            serviceCollection.AddSingleton(p => endpoint.Result)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>());
            return serviceCollection;
        }
    }
}
