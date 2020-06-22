using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities.AcceptanceTests.Bindings
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

            _ = host.ConfigureServices((s) =>
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
                  s.AddTransient(i => _context.CommandHandlerHooks);
                  s.AddTransient<CommandHandlerWithTestHook<AddLegalEntityCommand>, CommandHandlerWithTestHook<AddLegalEntityCommand>>();
                  s.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithTestHook<AddLegalEntityCommand>>();
              });

            host.UseEnvironment("LOCAL");
                      
            _context.FunctionsHost = await host.StartAsync();
        }
    }
}
