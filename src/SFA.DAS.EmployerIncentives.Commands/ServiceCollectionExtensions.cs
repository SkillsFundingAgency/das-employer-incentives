using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using NServiceBus.Persistence;
using Polly;
using Polly.Extensions.Http;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendEmploymentCheckRequests;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar.SetActivePeriodToInProgress;
using SFA.DAS.EmployerIncentives.Commands.Decorators;
using SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Persistence.Decorators;
using SFA.DAS.EmployerIncentives.Commands.Services;
using SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi;
using SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Data.Account;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.Services;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
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
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddDistributedLockProvider()
                .AddLearnerService()
                .AddEmploymentCheckService();

            serviceCollection
                .AddCommandHandlers(addDecorators: AddCommandHandlerDecorators)
                .AddDomainCommandHandlerValidators()
                .AddScoped<ICommandDispatcher, CommandDispatcher>()
                .Decorate<IUnitOfWorkManager, UnitOfWorkManagerWithScope>()
                .Decorate<ICommandDispatcher, CommandDispatcherWithLogging>()
                .Decorate<ICommandDispatcher, CommandDispatcherWithLoggingArgs>();

            serviceCollection
              .AddSingleton(c => new Policies(c.GetService<IOptions<PolicySettings>>()));

            serviceCollection.AddScoped<IIncentiveApplicationFactory, IncentiveApplicationFactory>();
            serviceCollection.AddScoped<IApprenticeshipIncentiveFactory, ApprenticeshipIncentiveFactory>();
            serviceCollection.AddScoped<ILearnerFactory, LearnerFactory>();
            serviceCollection.AddScoped<ICollectionCalendarService, CollectionCalendarService>();
            serviceCollection.AddSingleton<IDateTimeService, DateTimeService>();

            serviceCollection.AddScoped<ILearnerService, LearnerService>()
                .Decorate<ILearnerService, LearnerServiceWithCache>();

            serviceCollection.AddScoped<ICommandPublisher, CommandPublisher>();
            serviceCollection.AddScoped<IScheduledCommandPublisher, ScheduledCommandPublisher>();

            serviceCollection.AddBusinessCentralClient<IBusinessCentralFinancePaymentsService>((c, s, version, limit, obfuscateSensitiveData) =>
                new BusinessCentralFinancePaymentsServiceWithLogging(new BusinessCentralFinancePaymentsService(c, limit, version, obfuscateSensitiveData), s.GetRequiredService<ILogger<BusinessCentralFinancePaymentsServiceWithLogging>>(), obfuscateSensitiveData));

            return serviceCollection;
        }

        public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection, Func<IServiceCollection, IServiceCollection> addDecorators = null)
        {
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
            });

            if (addDecorators != null)
            {
                serviceCollection = addDecorators(serviceCollection);
            }

            return serviceCollection;
        }

        public static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IAccountDataRepository, AccountDataRepository>();
            serviceCollection.AddScoped<IPaymentDataRepository, PaymentDataRepository>();            
            serviceCollection.AddScoped<IAccountDomainRepository, AccountDomainRepository>();
            serviceCollection.AddScoped<IApprenticeApplicationDataRepository, ApprenticeApplicationDataRepository>();

            serviceCollection.AddScoped<IIncentiveApplicationDataRepository, IncentiveApplicationDataRepository>();
            serviceCollection.AddScoped<IIncentiveApplicationDomainRepository, IncentiveApplicationDomainRepository>();

            serviceCollection.AddScoped<IApprenticeshipIncentiveDataRepository, ApprenticeshipIncentiveDataRepository>();
            serviceCollection.AddScoped<IApprenticeshipIncentiveDomainRepository, ApprenticeshipIncentiveDomainRepository>();

            serviceCollection.AddScoped<ICollectionPeriodDataRepository, CollectionPeriodDataRepository>();
            serviceCollection.AddScoped<IAcademicYearDataRepository, AcademicYearDataRepository>();
            serviceCollection.AddScoped<ILearnerDataRepository, LearnerDataRepository>();
            serviceCollection.AddScoped<ILearnerDomainRepository, LearnerDomainRepository>();
            serviceCollection.Decorate<ILearnerDomainRepository, LearnerDomainRepositoryWithLogging>();

            serviceCollection.AddScoped<IIncentiveApplicationStatusAuditDataRepository, IncentiveApplicationStatusAuditDataRepository>();
            serviceCollection.AddScoped<IApprenticeshipIncentiveArchiveRepository, ApprenticeshipIncentiveArchiveRepository>();
            serviceCollection.AddScoped<IChangeOfCircumstancesDataRepository, ChangeOfCircumstancesDataRepository>();            

            serviceCollection.AddScoped<IApprenticeshipIncentiveArchiveRepository, ApprenticeshipIncentiveArchiveRepository>();
            serviceCollection.AddScoped<IEmploymentCheckAuditRepository, EmploymentCheckAuditRepository>();
            serviceCollection.AddScoped<IVendorBlockAuditRepository, VendorBlockAuditRepository>();

            serviceCollection.AddScoped<IValidationOverrideAuditRepository, ValidationOverrideAuditRepository>();
            serviceCollection.AddScoped<IRevertedPaymentAuditRepository, RevertedPaymentAuditRepository>();
            serviceCollection.AddScoped<IReinstatedPendingPaymentAuditRepository, ReinstatedPendingPaymentAuditRepository>();
            
            return serviceCollection;
        }

        public static IServiceCollection AddCommandHandlerDecorators(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithUnitOfWork<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithDistributedLock<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithRetry<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithDistributedLockInitialiser<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithPeriodEndDelay<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithValidator<>))
                .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithLogging<>))
                .Decorate(typeof(ICommandHandler<SendEmploymentCheckRequestsCommand>), typeof(SendEmploymentCheckRequestsCommandHandlerWithEmploymentCheckToggle));

            return serviceCollection;
        }
             
        public static IServiceCollection AddDomainCommandHandlerValidators(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton(typeof(IValidator<CalculateEarningsCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<ValidatePendingPaymentCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CompleteEarningsCalculationCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CreatePaymentCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<RefreshLearnerCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<LearnerChangeOfCircumstanceCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CalculateDaysInLearningCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<EarningsResilienceApplicationsCheckCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<EarningsResilienceIncentivesCheckCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SendPaymentRequestsCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<WithdrawCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<ReinstateApprenticeshipIncentiveCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<UpdateVendorRegistrationCaseStatusForAccountCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SendClawbacksCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SetSuccessfulLearnerMatchCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<CompleteCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SetActivePeriodToInProgressCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<UpdateEmploymentCheckCommand>), new NullValidator())                
                .AddSingleton(typeof(IValidator<SendEmploymentCheckRequestsCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<RefreshEmploymentCheckCommand>), new NullValidator())
                .AddSingleton(typeof(IValidator<SendMetricsReportCommand>), new NullValidator())
                ;

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

        private static IServiceCollection AddBusinessCentralClient<T>(this IServiceCollection serviceCollection, Func<HttpClient, IServiceProvider, string, int, bool, T> instance) where T : class
        {
            serviceCollection.AddTransient(s =>
            {
                var settings = s.GetService<IOptions<BusinessCentralApiClient>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithApimAuthorisationHeader(settings)
                    .WithLogging(s.GetService<ILoggerFactory>())
                    .WithHandler(new TransientRetryHandler(p => p.RetryAsync(3)));

                var httpClient = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }
                httpClient.BaseAddress = new Uri(settings.ApiBaseUrl);

                return instance.Invoke(httpClient, s, settings.ApiVersion, settings.PaymentRequestsLimit, settings.ObfuscateSensitiveData);
            });

            return serviceCollection;
        }

        private static IServiceCollection AddLearnerService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ILearnerSubmissionService>(s =>
            {
                var settings = s.GetService<IOptions<MatchedLearnerApi>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithLogging(s.GetService<ILoggerFactory>());

                if (!string.IsNullOrEmpty(settings.IdentifierUri))
                {
                    clientBuilder.WithManagedIdentityAuthorisationHeader(new ManagedIdentityTokenGenerator(settings));
                }

                var client = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }

                client.BaseAddress = new Uri(settings.ApiBaseUrl);

                return new LearnerSubmissionService(client, settings.Version);
            });

            return serviceCollection;
        }

        private static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEmploymentCheckService>(s =>
            {
                var settings = s.GetService<IOptions<EmployerIncentivesOuterApi>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithDefaultHeaders()
                    .WithApimAuthorisationHeader(settings)
                    .WithLogging(s.GetService<ILoggerFactory>());

                var client = clientBuilder.Build();

                if (!settings.ApiBaseUrl.EndsWith("/"))
                {
                    settings.ApiBaseUrl += "/";
                }

                client.BaseAddress = new Uri(settings.ApiBaseUrl);

                return new EmploymentCheckService(client, settings.ApiVersion);
            });

            return serviceCollection;
        }        

        public static async Task<UpdateableServiceProvider> StartNServiceBus(
            this UpdateableServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var endpointName = configuration["ApplicationSettings:NServiceBusEndpointName"];
            if (string.IsNullOrEmpty(endpointName))
            {
                endpointName = "SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers";
            }

            var endpointConfiguration = new EndpointConfiguration(endpointName)
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
                    .StorageDirectory(configuration["ApplicationSettings:UseLearningEndpointStorageDirectory"] ??
                        Path.Combine(
                            Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")),
                            @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport"));

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
                EmployerIncentivesDbContext dbContext;
                var unitOfWorkContext = p.GetService<IUnitOfWorkContext>();
                var synchronizedStorageSession = unitOfWorkContext.Find<SynchronizedStorageSession>();
                if(synchronizedStorageSession != null)
                {
                    var sqlStorageSession = synchronizedStorageSession.GetSqlStorageSession();
                    var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseSqlServer(sqlStorageSession.Connection);
                    dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options);
                    dbContext.Database.UseTransaction(sqlStorageSession.Transaction);
                }
                else
                {
                    var settings = p.GetService<IOptions<ApplicationSettings>>();
                    var optionsBuilder = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseSqlServer(settings.Value.DbConnectionString);
                    dbContext = new EmployerIncentivesDbContext(optionsBuilder.Options);
                }

                return dbContext;
            });
        }

        public sealed class TransientRetryHandler : PolicyHttpMessageHandler
        {
            public TransientRetryHandler(Func<PolicyBuilder<HttpResponseMessage>, IAsyncPolicy<HttpResponseMessage>> configurePolicy)
                : base(configurePolicy(HttpPolicyExtensions.HandleTransientHttpError()))
            {
            }
        }
    }
}
