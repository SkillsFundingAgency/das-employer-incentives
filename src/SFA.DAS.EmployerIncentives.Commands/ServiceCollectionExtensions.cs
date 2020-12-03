using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using NServiceBus.Persistence;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Persistence.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using SFA.DAS.HashingService;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.NServiceBus.SqlServer.Data;
using SFA.DAS.UnitOfWork.Context;
using SFA.DAS.UnitOfWork.Managers;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using SFA.DAS.EmployerIncentives.Commands.Persistence.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddDistributedLockProvider()
                .AddHashingService()
                .AddAccountService()
                .AddLearnerService();

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
            .AddScoped<ICommandDispatcher, CommandDispatcher>()
            .Decorate<IUnitOfWorkManager, UnitOfWorkManagerWithScope>()            
            .Decorate<ICommandDispatcher, CommandDispatcherWithLogging>();

            serviceCollection
              .AddSingleton(c => new Policies(c.GetService<IOptions<PolicySettings>>()));

            serviceCollection.AddScoped<IIncentiveApplicationFactory, IncentiveApplicationFactory>();
            serviceCollection.AddScoped<IApprenticeshipIncentiveFactory, ApprenticeshipIncentiveFactory>();
            serviceCollection.AddScoped<ILearnerFactory, LearnerFactory>();

            serviceCollection.AddSingleton<IIncentivePaymentProfilesService, IncentivePaymentProfilesService>();
            serviceCollection.AddScoped<ICollectionCalendarService, CollectionCalendarService>();

            serviceCollection.AddScoped<ICommandPublisher, CommandPublisher>();

            serviceCollection.AddBusinessCentralClient<IBusinessCentralFinancePaymentsService>((c, s, version, limit) => new BusinessCentralFinancePaymentsService(c, limit, version));

            return serviceCollection;
        }

        public static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAccountDataRepository, AccountDataRepository>();
            serviceCollection.AddScoped<IAccountDomainRepository, AccountDomainRepository>();
            serviceCollection.AddScoped<IApprenticeApplicationDataRepository, ApprenticeApplicationDataRepository>();

            serviceCollection.AddScoped<IIncentiveApplicationDataRepository, IncentiveApplicationDataRepository>();
            serviceCollection.AddScoped<IIncentiveApplicationDomainRepository, IncentiveApplicationDomainRepository>();

            serviceCollection.AddScoped<IApprenticeshipIncentiveDataRepository, ApprenticeshipIncentiveDataRepository>();
            serviceCollection.AddScoped<IApprenticeshipIncentiveDomainRepository, ApprenticeshipIncentiveDomainRepository>();

            serviceCollection.AddScoped<ICollectionPeriodDataRepository, CollectionPeriodDataRepository>();
            serviceCollection.AddScoped<ILearnerDataRepository, LearnerDataRepository>();
            serviceCollection.AddScoped<ILearnerDomainRepository, LearnerDomainRepository>();
            serviceCollection.Decorate<ILearnerDomainRepository, LearnerDomainRepositoryWithLogging>();

            return serviceCollection;
        }

        public static IServiceCollection AddCommandHandlerDecorators(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithUnitOfWork<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithDistributedLock<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithRetry<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithValidator<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithLogging<>));

            serviceCollection
                .AddSingleton(typeof(IValidator<CreateIncentiveCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CalculateEarningsCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<ValidatePendingPaymentCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CompleteEarningsCalculationCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CreatePaymentCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<RefreshLearnerCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CalculateDaysInLearningCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<EarningsResilienceApplicationsCheckCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<EarningsResilienceIncentivesCheckCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SendPaymentRequestsCommand>), new NullValidator());

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
            serviceCollection.AddSingleton<IHashingService>(c =>
            {
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

                return new AccountService(client);
            });

            return serviceCollection;
        }

        private static IServiceCollection AddBusinessCentralClient<T>(this IServiceCollection serviceCollection, Func<HttpClient, IServiceProvider, string, int, T> instance) where T : class
        {
            serviceCollection.AddTransient(s =>
            {
                var settings = s.GetService<IOptions<BusinessCentralApiClient>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithApimAuthorisationHeader(settings)
                    .WithLogging(s.GetService<ILoggerFactory>());

                var httpClient = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }
                httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);

                return instance.Invoke(httpClient, s, settings.ApiVersion, settings.PaymentRequestsLimit);
            });

            return serviceCollection;
        }

        public static IServiceCollection AddLearnerService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ILearnerService>(s =>
            {
                var settings = s.GetService<IOptions<MatchedLearnerApi>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithLogging(s.GetService<ILoggerFactory>());

                if (!string.IsNullOrEmpty(settings.Identifier))
                {
                    clientBuilder.WithManagedIdentityAuthorisationHeader(new ManagedIdentityTokenGenerator(settings));
                }

                var client = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }

                client.BaseAddress = new Uri(settings.ApiBaseUrl);

                return new LearnerService(client, settings.Version);
            });

            return serviceCollection;
        }

        public static async Task<UpdateableServiceProvider> StartNServiceBus(
            this UpdateableServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer()
                .UseOutbox(true)
                .UseServicesBuilder(serviceProvider)
                .UseSqlServerPersistence(() => new SqlConnection(configuration["ApplicationSettings:DbConnectionString"]))
                .UseUnitOfWork();

            if (configuration["ApplicationSettings:NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                endpointConfiguration
                    .UseTransport<LearningTransport>()
                    .StorageDirectory(configuration.GetValue("ApplicationSettings:UseLearningEndpointStorageDirectory",
                        Path.Combine(
                            Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")),
                            @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport")));
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration
                    .UseAzureServiceBusTransport(configuration["ApplicationSettings:NServiceBusConnectionString"], r => r.AddRouting());
            }

            if (!string.IsNullOrEmpty(configuration["ApplicationSettings:NServiceBusLicense"]))
            {
                endpointConfiguration.License(configuration["ApplicationSettings:NServiceBusLicense"]);
            }

            var endpoint = await Endpoint.Start(endpointConfiguration);

            serviceProvider.AddSingleton(p => endpoint)
                .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();

            return serviceProvider;
        }

        public static IServiceCollection AddEntityFrameworkForEmployerIncentives(this IServiceCollection services)
        {
            return services.AddScoped(p =>
            {
                var unitOfWorkContext = p.GetService<IUnitOfWorkContext>();
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
                    var settings = p.GetService<IOptions<ApplicationSettings>>();
                    var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseSqlServer(settings.Value.DbConnectionString);
                    dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options);
                }

                return dbContext;
            });
        }

    }
}
