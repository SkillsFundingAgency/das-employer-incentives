using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities;
using SFA.DAS.EmployerIncentives.Queries.Decorators;

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
                .AddTransient<IUlnValidationService, UlnValidationService>()
                .AddTransient<IIncentiveApplicationQueryRepository, IncentiveApplicationQueryRepository>()
                .AddQueryHandlerDecorators()
                .AddScoped<IQueryDispatcher, QueryDispatcher>()
                .AddScoped<INewApprenticeIncentiveEligibilityService, NewApprenticeIncentiveEligibilityService>()
                .AddScoped<IPaymentsQueryRepository, PaymentsQueryRepository>()
                .AddScoped<IApprenticeshipIncentiveQueryRepository, ApprenticeshipIncentiveQueryRepository>()
                .AddSingleton(c => new Policies(c.GetService<IOptions<PolicySettings>>()));

            return serviceCollection;
        }

        public static IServiceCollection AddQueryHandlerDecorators(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerWithRetry<,>))
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerWithLogging<,>))
                .Decorate(typeof(IQueryHandler<,>), typeof(QueryHandlerWithLoggingArgs<,>));

            return serviceCollection;
        }

    }
}
