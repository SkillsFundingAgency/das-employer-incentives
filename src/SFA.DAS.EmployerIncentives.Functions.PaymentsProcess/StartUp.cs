﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.SqlServer.DependencyResolution.Microsoft;
using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NServiceBus;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.NServiceBus.SqlServer.Features.ClientOutbox.Data;
using SFA.DAS.NServiceBus.SqlServer.Features.ClientOutbox.StartupTasks;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Services;

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
            builder.Services.Configure<EmploymentCheckApi>(config.GetSection("EmploymentCheckApi"));

            builder.Services.AddNLog(config);

            builder.Services.AddEntityFrameworkForEmployerIncentives()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddNServiceBusClientUnitOfWork();
            
            builder.Services.AddPersistenceServices();
            builder.Services.AddQueryServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();

            builder.Services.AddNServiceBus(config);
        }
    }
}