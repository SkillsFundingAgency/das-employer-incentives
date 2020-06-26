using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity;
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
            serviceCollection.AddSingleton<IDistributedLockProvider, AzureDistributedLockProvider>(s =>
               new AzureDistributedLockProvider(
                   s.GetRequiredService<IOptions<ApplicationSettings>>(),
                   s.GetRequiredService<ILogger<AzureDistributedLockProvider>>(),
                   "employer-incentives-distributed-locks"));
            serviceCollection.AddSingleton(c => new Policies(c.GetService<IOptions<RetryPolicies>>()));

            serviceCollection.AddSingleton<IValidator<AddLegalEntityCommand>, AddLegalEntityCommandValidator>();
            serviceCollection.AddTransient<ICommandHandler<AddLegalEntityCommand>, AddLegalEntityCommandHandler>();
            serviceCollection.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithDistributedLock<AddLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithRetry<AddLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithValidator<AddLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithLogging<AddLegalEntityCommand>>();

            serviceCollection.AddSingleton<IValidator<RemoveLegalEntityCommand>, RemoveLegalEntityCommandValidator>();
            serviceCollection.AddTransient<ICommandHandler<RemoveLegalEntityCommand>, RemoveLegalEntityCommandHandler>();
            serviceCollection.Decorate<ICommandHandler<RemoveLegalEntityCommand>, CommandHandlerWithDistributedLock<RemoveLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<RemoveLegalEntityCommand>, CommandHandlerWithRetry<RemoveLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<RemoveLegalEntityCommand>, CommandHandlerWithValidator<RemoveLegalEntityCommand>>();
            serviceCollection.Decorate<ICommandHandler<RemoveLegalEntityCommand>, CommandHandlerWithLogging<RemoveLegalEntityCommand>>();

            serviceCollection.AddTransient<IAccountDomainRepository, AccountDomainRepository>();
            serviceCollection.AddTransient<IAccountDataRepository, AccountDataRepository>();

            return serviceCollection;
        }
    }
}
