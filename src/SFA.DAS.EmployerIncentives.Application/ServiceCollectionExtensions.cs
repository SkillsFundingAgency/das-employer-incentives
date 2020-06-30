using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Decorators;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Application
{
    public static class ServiceCollectionExtensions
    {       
        public static IServiceCollection AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDistributedLockProvider();
            serviceCollection.AddSingleton(c => new Policies(c.GetService<IOptions<RetryPolicies>>()));

            // set up the command handlers and command validators
            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ICommandHandler<>))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()

                    .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime();
            })
            .AddCommandHandlerDecorators();

            serviceCollection.AddTransient<IAccountDomainRepository, AccountDomainRepository>();
            serviceCollection.AddTransient<IAccountDataRepository, AccountDataRepository>();

            serviceCollection.AddSingleton<ICommandDispatcher, CommandDispatcher>();

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
    }
}
