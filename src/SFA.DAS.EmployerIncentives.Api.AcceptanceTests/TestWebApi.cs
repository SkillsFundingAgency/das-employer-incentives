﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.UnitOfWork.Context;
using SFA.DAS.UnitOfWork.Managers;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestWebApi : WebApplicationFactory<Startup>
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _config;

        public TestWebApi(TestContext context)
        {
            _context = context;

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
                    e.BankDetailsReminder = new EmailTemplate { TemplateId = Guid.NewGuid().ToString() };
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

                s.AddTransient<IUnitOfWorkContext>(c => new TestUnitOfWorkContext(_context));
                s.AddTransient<IUnitOfWorkManager>(c => new TestUnitOfWorkManager());

                s.AddTransient<IDistributedLockProvider, NullLockProvider>();

                s.UseTestDb(_context);
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
