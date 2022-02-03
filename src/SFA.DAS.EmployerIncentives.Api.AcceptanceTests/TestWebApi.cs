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
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestWebApi : WebApplicationFactory<Startup>
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _config;
        private readonly IHook<object> _eventMessageHook;
        private readonly IHook<ICommand> _commandMessageHook;

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
                    { "ApplicationSettings:LogLevel", "Info" },
                    { "ConfigNames", "SFA.DAS.EmployerIncentives" }
                };
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.Configure<ApplicationSettings>(a =>
                {
                    a.DbConnectionString = _context.ApplicationSettings.DbConnectionString;
                    a.DistributedLockStorage = _context.ApplicationSettings.DistributedLockStorage;
                    a.AllowedHashstringCharacters = _context.ApplicationSettings.AllowedHashstringCharacters;
                    a.Hashstring = _context.ApplicationSettings.Hashstring;
                    a.NServiceBusConnectionString = _context.ApplicationSettings.NServiceBusConnectionString;
                    a.MinimumAgreementVersion = _context.ApplicationSettings.MinimumAgreementVersion;
                    a.EmploymentCheckEnabled = _context.ApplicationSettings.EmploymentCheckEnabled;
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
                    e.ApplicationCancelled = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
                });
                s.Configure<MatchedLearnerApi>(l =>
                {
                    l.ApiBaseUrl = _context.LearnerMatchApi.BaseAddress;
                    l.Identifier = "";
                    l.Version = "1.0";
                });
                s.Configure<EmploymentCheckApi>(l =>
                {
                    l.ApiBaseUrl = _context.EmploymentCheckApi.BaseAddress;
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
                s.Decorate<IScheduledCommandPublisher>((handler, sp) => new TestScheduledCommandPublisher(handler, _commandMessageHook));
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
