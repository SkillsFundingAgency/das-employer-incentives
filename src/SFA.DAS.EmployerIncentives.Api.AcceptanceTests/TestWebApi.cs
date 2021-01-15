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
                    { "ConfigNames", "SFA.DAS.EmployerIncentives" }
                };

            _paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile
                {
                    IncentiveType = Enums.IncentiveType.TwentyFiveOrOverIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                    {
                        new PaymentProfile{ AmountPayable = 100, DaysAfterApprenticeshipStart = 10},
                        new PaymentProfile{ AmountPayable = 200, DaysAfterApprenticeshipStart = 20},
                    }
                },
                new IncentivePaymentProfile
                {
                    IncentiveType = Enums.IncentiveType.UnderTwentyFiveIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                    {
                        new PaymentProfile{ AmountPayable = 300, DaysAfterApprenticeshipStart = 30},
                        new PaymentProfile{ AmountPayable = 400, DaysAfterApprenticeshipStart = 40},
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

                s.AddTransient<IDistributedLockProvider, NullLockProvider>();
                s.Decorate<IEventPublisher>((handler, sp) => new TestEventPublisher(handler, _eventMessageHook));
                s.Decorate<ICommandPublisher>((handler, sp) => new TestCommandPublisher(handler, _commandMessageHook));
            });
            builder.ConfigureAppConfiguration(a =>
            {
                a.Sources.Clear();
                a.AddInMemoryCollection(_config);
            });
            builder.UseEnvironment("LOCAL");
        }
    }
}
