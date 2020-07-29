using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.HashingService;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public static class ServiceCollectionExtensions
    { 
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddDistributedLockProvider()
                .AddHashingService()
                .AddNServiceBusUnitOfWork()
                .AddNServiceBusClientUnitOfWork()
                .AddAccountService();
            
            // set up the command handlers and command validators
            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()

                    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime();
            })
            .AddCommandHandlerDecorators()
            .AddScoped<ICommandDispatcher, CommandDispatcher>();

            serviceCollection
              .AddSingleton(c => new Policies(c.GetService<IOptions<PolicySettings>>()));

            serviceCollection.AddScoped<IAccountDataRepository, AccountDataRepository>();
            serviceCollection.AddScoped<IAccountDomainRepository, AccountDomainRepository>();

            serviceCollection.AddScoped<IIncentiveApplicationDataRepository, IncentiveApplicationDataRepository>();
            serviceCollection.AddScoped<IIncentiveApplicationDomainRepository, IncentiveApplicationDomainRepository>();
            serviceCollection.AddScoped<IIncentiveApplicationFactory, IncentiveApplicationFactory>();

            serviceCollection.AddScoped<IMultiEventPublisher, MultiEventPublisherWithLimit>();

            return serviceCollection;
        }

        public static IServiceCollection AddCommandHandlerDecorators(this IServiceCollection serviceCollection)        
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithDistributedLock<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithRetry<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithValidator<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithLogging<>));

            return serviceCollection;
        }

        public static IServiceCollection AddDistributedLockProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDistributedLockProvider, AzureDistributedLockProvider>(s =>
               new AzureDistributedLockProvider(
                   s.GetRequiredService<IOptions<ApplicationSettings>>(),
                   s.GetRequiredService<ILogger<AzureDistributedLockProvider>>(),
                   "employer-incentives-distributed-locks"));

            return serviceCollection;
        }

        public static IServiceCollection AddHashingService(this IServiceCollection serviceCollection)
        {   
            serviceCollection.AddSingleton<IHashingService>(c => {
                var settings = c.GetService<IOptions<ApplicationSettings>>().Value;
                return new HashingService.HashingService(settings.AllowedHashstringCharacters, settings.Hashstring);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddAccountService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IAccountService>(s =>
            {
                var settings = s.GetService<IOptions<AccountApi>>().Value;

                var clientBuilder = new HttpClientBuilder()                
                    .WithDefaultHeaders()
                    .WithLogging(s.GetService<ILoggerFactory>());

                if (!string.IsNullOrEmpty(settings.ClientId))
                {
                    clientBuilder.WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(settings));
                }

                var client = clientBuilder.Build();

                client.BaseAddress = new Uri(settings.ApiBaseUrl);

                return new AccountService(client, s.GetRequiredService<IHashingService>());
            });

            return serviceCollection;
        }

        public static async Task StartNServiceBus(
            this UpdateableServiceProvider serviceProvider,
            IConfiguration configuration)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Api")
                .UseErrorQueue()
#pragma warning restore CS0618 // Type or member is obsolete
                //.UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseServicesBuilder(serviceProvider)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["ApplicationSettings:DbConnectionString"]))
                .UseUnitOfWork();

            if (configuration["ApplicationSettings:NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                endpointConfiguration.UseLearningTransport();
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(
                    configuration["ApplicationSettings:NServiceBusConnectionString"], r => { });
            }

            if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
            {
                endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
            }

            var endpoint = await Endpoint.Start(endpointConfiguration);

            serviceProvider.AddSingleton(p => endpoint)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}
