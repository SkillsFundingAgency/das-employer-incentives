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
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SFA.DAS.EmployerIncentives.Reports;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net;

[assembly: FunctionsStartup(typeof(Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
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

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.Configure<PolicySettings>(config.GetSection("PolicySettings"));
            builder.Services.Configure<MatchedLearnerApi>(config.GetSection("MatchedLearnerApi"));
            builder.Services.Configure<BusinessCentralApiClient>(config.GetSection("BusinessCentralApi"));
            builder.Services.Configure<EmployerIncentivesOuterApi>(config.GetSection("EmployerIncentivesOuterApi"));
            builder.Services.Configure<EmailTemplateSettings>(config.GetSection("EmailTemplates"));
            builder.Services.Configure<PaymentProcessSettings>(config.GetSection("PaymentProcess"));

            builder.Services.AddNLog(config);

            builder.Services.AddEntityFrameworkForEmployerIncentives()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddNServiceBusClientUnitOfWork();

            builder.Services.AddPersistenceServices();
            builder.Services.AddQueryServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();
            builder.Services.AddReportServices();

            builder.Services.AddNServiceBus(config);
        }
    }
}