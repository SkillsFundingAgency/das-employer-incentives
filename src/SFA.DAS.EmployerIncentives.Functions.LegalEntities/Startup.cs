using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmployerIncentives.Functions.LegalEntities.Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddNLog();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureTableStorage(options =>
                {   
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["Environment"];
                    options.PreFixConfigurationKeys = false;
                })
                .Build();


            builder.AddExecutionContextBinding();
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            if (configuration["Environment"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                builder.Services.AddNServiceBus(logger, Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport"));
            }
            else
            {
                builder.Services.AddNServiceBus(logger);
            }

            builder.Services.AddOptions();
            builder.Services.Configure<FunctionSettings>(config.GetSection("FunctionSettings"));

            builder.Services.AddTransient<ICommandHandler<AddLegalEntityCommand>, AddLegalEntityCommandHandler>();
            builder.Services.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithLogging<AddLegalEntityCommand>>();

            builder.Services.AddTransient<IValidator<AddLegalEntityCommand>, AddLegalEntityCommandValidator>();
            builder.Services.AddTransient<IDomainRepository<long, Account>, AccountDomainRepository>();

            builder.Services.AddTransient<IAccountDataRepository, AccountDataRepository>();
            
        }
    }
}
