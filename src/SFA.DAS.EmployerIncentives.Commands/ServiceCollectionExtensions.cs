using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.HashingService;
using SFA.DAS.Http;
using SFA.DAS.NLog.Logger.Web.MessageHandlers;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.Microsoft;
using System;
using static SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities.RefreshLegalEntitiesCommandHandler;

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
              .AddSingleton(c => new Policies(c.GetService<IOptions<RetryPolicies>>()));

            serviceCollection.AddScoped<IAccountDataRepository, AccountDataRepository>();
            serviceCollection.AddScoped<IAccountDomainRepository, AccountDomainRepository>();            

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

                var client = new HttpClientBuilder()
                    //.WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(settings))
                    .WithDefaultHeaders()
                    .WithLogging(s.GetService<ILoggerFactory>())
                    .WithHandler(new RequestIdMessageRequestHandler())
                    .WithHandler(new SessionIdMessageRequestHandler())
                    .Build();

                client.BaseAddress = new Uri(settings.ApiBaseUrl);

                return new AccountService(client, s.GetRequiredService<IHashingService>());
            });

            return serviceCollection;
        }
    }
}
