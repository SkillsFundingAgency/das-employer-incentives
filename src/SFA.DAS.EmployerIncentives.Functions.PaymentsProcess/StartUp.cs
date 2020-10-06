using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class Startup : FunctionsStartup
    {   
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Add NLog

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (!ConfigurationIsLocalOrAcceptanceTests(configuration))
            {
                // Add azure storage config
            }
#if DEBUG
            if (!configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                configBuilder.AddJsonFile($"local.settings.json", optional: true);
            }
#endif
            var config = configBuilder.Build();

            builder.Services.AddOptions();            
            
            // Add application settings
            //builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
        }

        private bool ConfigurationIsLocalOrAcceptanceTests(IConfiguration configuration)
        {
            return configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   configuration["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}