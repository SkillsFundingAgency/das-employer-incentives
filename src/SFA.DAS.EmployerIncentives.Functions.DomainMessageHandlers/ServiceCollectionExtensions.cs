using Microsoft.Azure.WebJobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Container;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public static class ServiceCollectionExtensions
    {   
        
        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            ILogger logger,
            Action<NServiceBusOptions> OnConfigureOptions = null)
        {            
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var options = new NServiceBusOptions
            {
                OnMessageReceived = (context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                    logger.LogInformation($"Received NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");

                },
                OnMessageErrored = (ex, context) =>
                {
                    context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                    context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                    context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                    context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                    logger.LogError(ex, $"Error handling NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
                }
            };

            if (OnConfigureOptions != null)
            {
                OnConfigureOptions.Invoke(options);
            }

            webBuilder.AddExtension(new NServiceBusExtensionConfigProvider(options));

            return serviceCollection;
        }

        public static IServiceCollection AddNServiceBus(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var webBuilder = serviceCollection.AddWebJobs(x => { });
            webBuilder.AddExecutionContextBinding();

            var endpointName = configuration["ApplicationSettings:NServiceBusEndpointName"];
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = "SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers";
            }

            var endpointConfiguration = new EndpointConfiguration(endpointName)
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
