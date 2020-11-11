using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Events.Decorators;
using SFA.DAS.EmployerIncentives.Events.Services;

namespace SFA.DAS.EmployerIncentives.Events
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>))
                    .NotInNamespaceOf(typeof(EventHandlerWithLogging<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            })
            .AddDomainEventHandlerDecorators()
            .AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            return serviceCollection;
        }

        public static IServiceCollection AddDomainEventHandlerDecorators(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(IDomainEventHandler<>), typeof(EventHandlerWithLogging<>));

            return serviceCollection;
        }
    }
}
