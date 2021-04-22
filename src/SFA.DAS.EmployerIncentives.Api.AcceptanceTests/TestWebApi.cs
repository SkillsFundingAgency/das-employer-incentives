using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.IO;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestWebApi : WebApplicationFactory<Startup>
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _config;
        private readonly IHook<object> _eventMessageHook;
        private readonly IHook<ICommand> _commandMessageHook;
        private readonly List<IncentivePaymentProfile> _paymentProfiles;

        public TestWebApi(TestContext context, IHook<object> eventMessageHook, IHook<ICommand> commandMessageHook)
        {
            _context = context;
            _eventMessageHook = eventMessageHook;
            _commandMessageHook = commandMessageHook;

            _config = new Dictionary<string, string>{
                    { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                    { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                    { "ApplicationSettings:EmployerIncentivesWebBaseUrl", "https://localhost:5001" },
                    { "ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true" },
                    { "ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(_context.TestDirectory.FullName, ".learningtransport") },
                    { "ApplicationSettings:DbConnectionString", _context.SqlDatabase.DatabaseInfo.ConnectionString },
                    { "ApplicationSettings:NServiceBusEndpointName", _context.InstanceId },
                    { "ConfigNames", "SFA.DAS.EmployerIncentives" }
                };

            _paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile
                {
                    IncentivePhase = IncentivePhase.Phase1_0,
                    EligibleApplicationDates = (new DateTime(2020,8,1), new DateTime(2021,5,31)),
                    EligibleTrainingDates = (new DateTime(2020,8,1), new DateTime(2021,5,31)),
                    MinRequiredAgreementVersion = 4,
                    PaymentProfiles = new List<PaymentProfile>
                    {
                        new PaymentProfile {AmountPayable = 1000, DaysAfterApprenticeshipStart = 89, IncentiveType = IncentiveType.UnderTwentyFiveIncentive},
                        new PaymentProfile {AmountPayable = 1000, DaysAfterApprenticeshipStart = 364, IncentiveType = IncentiveType.UnderTwentyFiveIncentive},
                        new PaymentProfile {AmountPayable = 750, DaysAfterApprenticeshipStart = 89,IncentiveType = IncentiveType.TwentyFiveOrOverIncentive},
                        new PaymentProfile {AmountPayable = 750, DaysAfterApprenticeshipStart = 364,IncentiveType = IncentiveType.TwentyFiveOrOverIncentive},
                    }
                }
            };

        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.Configure<ApplicationSettings>(a =>
                {
                    a.DbConnectionString = _context.SqlDatabase.DatabaseInfo.ConnectionString;
                    a.DistributedLockStorage = "UseDevelopmentStorage=true";
                    a.AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY";
                    a.Hashstring = "Test Hashstring";
                    a.NServiceBusConnectionString = "UseLearningEndpoint=true";
                    a.MinimumAgreementVersion = 4;
                    a.IncentivePaymentProfiles = _paymentProfiles;
                });
                s.Configure<PolicySettings>(a =>
                {
                    a.RetryPolicies = new RetryPolicySettings
                    {
                        LockedRetryAttempts = 0,
                        LockedRetryWaitInMilliSeconds = 0,
                        QueryRetryWaitInMilliSeconds = 0,
                        QueryRetryAttempts = 0
                    };
                });
                s.Configure<EmailTemplateSettings>(e =>
                {
                    e.BankDetailsReminder = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
                    e.BankDetailsRequired = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
                    e.BankDetailsRepeatReminder = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
                });
                s.Configure<MatchedLearnerApi>(l =>
                {
                    l.ApiBaseUrl = _context.LearnerMatchApi.BaseAddress;
                    l.Identifier = "";
                    l.Version = "1.0";
                });
                if (_context.AccountApi != null)
                {
                    s.Configure<AccountApi>(a =>
                    {
                        a.ApiBaseUrl = _context.AccountApi.BaseAddress;
                        a.ClientId = "";
                    });
                }

                Commands.ServiceCollectionExtensions.AddCommandHandlers(s, AddDecorators);

                s.AddTransient<IDistributedLockProvider, NullLockProvider>();
                s.Decorate<IEventPublisher>((handler, sp) => new TestEventPublisher(handler, _eventMessageHook));
                s.Decorate<ICommandPublisher>((handler, sp) => new TestCommandPublisher(handler, _commandMessageHook));
                s.AddSingleton(_commandMessageHook);
            });
            builder.ConfigureAppConfiguration(a =>
            {
                a.Sources.Clear();
                a.AddInMemoryCollection(_config);
            });
            builder.UseEnvironment("LOCAL");
        }

        public IServiceCollection AddDecorators(IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerReceived<>));

            Commands.ServiceCollectionExtensions.AddCommandHandlerDecorators(serviceCollection);

            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerProcessed<>));

            return serviceCollection;
        }
    }
}
