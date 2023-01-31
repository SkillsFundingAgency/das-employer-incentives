using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NServiceBus.Persistence;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Events;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;
using SFA.DAS.UnitOfWork.EntityFrameworkCore.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using System;
using System.Collections.Generic;
using System.IO;

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

            builder.Services.AddNLog(config);

            builder.Services.AddMemoryCache();            

            builder.Services.AddDbContext<EmployerIncentivesDbContext>()
                .AddEntityFrameworkUnitOfWork<EmployerIncentivesDbContext>()
                .AddNServiceBusClientUnitOfWork();

            builder.Services.AddPersistenceServices();
            builder.Services.AddQueryServices();
            builder.Services.AddCommandServices();
            builder.Services.AddEventServices();

            builder.Services.AddNServiceBus(config);
        }

        public new void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<DbConfigProvider>();
            
            base.Configure(builder);
        }
    }

    [Extension("DbSeed")]
    internal class DbConfigProvider : IExtensionConfigProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbConfigProvider(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var unitOfWorkContext = serviceProvider.GetService<IUnitOfWorkContext>();
            EmployerIncentivesDbContext dbContext;
            try
            {
                var synchronizedStorageSession = unitOfWorkContext.Get<SynchronizedStorageSession>();
                var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
                var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseSqlServer(sqlStorageSession.Connection);

                dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options);
                dbContext.Database.UseTransaction(sqlStorageSession.Transaction);
            }
            catch (KeyNotFoundException)
            {
                var settings = serviceProvider.GetService<IOptions<ApplicationSettings>>();
                var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>();
                dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options, serviceProvider);
            }
            dbContext.Database.EnsureCreated();
        }
    }
}