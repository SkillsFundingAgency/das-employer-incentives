using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using SFA.DAS.Configuration.AzureTableStorage;

namespace FunctionApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var Configuration = new ConfigurationBuilder()
                                .AddInMemoryCollection()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddEnvironmentVariables()
                                .AddJsonFile("local.settings.json", optional: true)
                                .Build();

            var hostBuilder = new HostBuilder();

            var host = hostBuilder
                .ConfigureHostConfiguration(c =>
                {
                    c.AddConfiguration(Configuration);
                })
                .UseNServiceBusContainer()
                .UseFunctionStartUp(Configuration)
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder
                        .AddApplicationInsights()
                        .AddApplicationInsightsLogger();
                })
                .ConfigureAppConfiguration(builder =>
                {   
                    if (!Configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
                    {
                        builder.AddAzureTableStorage(options =>
                        {
                            options.ConfigurationKeys = Configuration["ConfigNames"].Split(",");
                            options.StorageConnectionString = Configuration["ConfigurationStorageConnectionString"];
                            options.EnvironmentName = Configuration["EnvironmentName"];
                            options.PreFixConfigurationKeys = false;
                        });
                    }

                }) 
                .Build();
 
 
            await host.RunAsync();
 
        }
    }
}