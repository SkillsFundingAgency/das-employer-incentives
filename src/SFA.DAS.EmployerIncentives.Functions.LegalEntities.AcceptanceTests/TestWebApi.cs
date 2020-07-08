using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Api;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    public class TestWebApi : WebApplicationFactory<Startup>
    {
        private readonly TestContext _context;
        private readonly Dictionary<string, string> _config;

        public TestWebApi(TestContext context)
        {
            _context = context;

            _config = new Dictionary<string, string>{
                    { "EnvironmentName", "LOCAL" },
                    { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
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
                });
                s.Configure<RetryPolicies>(a =>
                {
                    a.LockedRetryAttempts = 0;
                    a.LockedRetryWaitInMilliSeconds = 0;
                });

                s.UseTestDb(_context);
            });
            builder.ConfigureAppConfiguration(a =>
            {
                a.AddInMemoryCollection(_config);
            });
            builder.UseEnvironment("LOCAL");
        }
    }
}
