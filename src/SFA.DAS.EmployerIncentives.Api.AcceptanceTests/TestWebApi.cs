using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.UnitOfWork.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestWebApi : WebApplicationFactory<Startup>
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _config;
        private readonly IHook<object> _eventMessageHook;

        public TestWebApi(TestContext context, IHook<object> eventMessageHook)
        {
            _context = context;
            _eventMessageHook = eventMessageHook;

            _config = new Dictionary<string, string>{
                    { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                    { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                    { "ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true" },
                    { "ApplicationSettings:DbConnectionString", _context.SqlDatabase.DatabaseInfo.ConnectionString },
                    { "ConfigNames", "SFA.DAS.EmployerIncentives" }
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
                    a.Hashstring = "SFA: digital apprenticeship service";
                    a.NServiceBusConnectionString = "UseLearningEndpoint=true";
                    a.MinimumAgreementVersion = 4;
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
                    e.BankDetailsReminder = new EmailTemplate { TemplateId = Guid.NewGuid().ToString()};
                    e.BankDetailsRequired = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
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

                s.Decorate<IEventPublisher>((handler, sp) =>
                {
                    return new TestEventPublisher(handler, _eventMessageHook);
                });

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
