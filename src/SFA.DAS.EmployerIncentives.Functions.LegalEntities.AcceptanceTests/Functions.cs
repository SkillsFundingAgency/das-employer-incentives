using TechTalk.SpecFlow;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using SFA.DAS.EmployerIncentives.Infrastructure;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests
{
    [Binding]
    public class Functions
    {
        private readonly TestContext _context;

        public Functions(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario()]
        public async Task InitialiseFunctions()
        {
            var startUp = new Startup();

            var config = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                { "ConfigNames", "SFA.DAS.EmployerIncentives.Jobs" }
            };

            var host = new HostBuilder()
                .ConfigureHostConfiguration(a =>
                {
                    a.AddInMemoryCollection(config);
                })
               .ConfigureWebJobs(startUp.Configure);

            host.ConfigureServices((s) =>
            {
                s.Configure<FunctionSettings>(a =>
                {
                    a.DbConnectionString = _context.DatabaseProperties.ConnectionString;
                    a.DistributedLockStorage = "UseDevelopmentStorage=true";
                });
                s.Configure<RetryPolicies>(a =>
                {
                    a.LockedRetryAttempts = 0;
                    a.LockedRetryWaitInMilliSeconds = 0;
                });

                s.AddNServiceBus(new LoggerFactory().CreateLogger<Functions>(), Path.Combine(_context.TestDirectory.FullName, ".learningtransport"));
            });

            host.UseEnvironment("LOCAL");
                      
            _context.FunctionsHost = await host.StartAsync();
        }

        [AfterScenario()]
        public void CleanUpFunctions()
        {
            _context.FunctionsHost.Dispose();
        }
    }
}
