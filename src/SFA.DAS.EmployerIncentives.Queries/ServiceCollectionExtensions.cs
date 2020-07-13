using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries.Account;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public static class ServiceCollectionExtensions
    {       
        public static IServiceCollection AddQueryServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Scan(scan =>
                {
                    scan.FromExecutingAssembly()
                        .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    scan.FromAssembliesOf(typeof(GetLegalEntitiesQueryHandler))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    scan.FromAssembliesOf(typeof(AccountQueryRepository))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryRepository<>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();
                })
                .AddSingleton<IQueryDispatcher, QueryDispatcher>()
                .AddSingleton(c => new Policies(c.GetService<IOptions<RetryPolicies>>()));

            return serviceCollection;
        }
    }
}
