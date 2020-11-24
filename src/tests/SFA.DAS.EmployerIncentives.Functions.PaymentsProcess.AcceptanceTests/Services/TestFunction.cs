using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestFunction : IDisposable
    {
        private readonly TestContext _testContext;
        private readonly Dictionary<string, string> _appConfig;
        private readonly IHost _host;
        private bool isDisposed;

        private IJobHost Jobs => _host.Services.GetService<IJobHost>();
        public string HubName { get; }
        public HttpResponseMessage LastResponse { get; private set; }

        public TestFunction(TestContext testContext, string hubName)
        {
            HubName = hubName;

            _appConfig = new Dictionary<string, string>{
                    { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                    { "AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                    { "NServiceBusConnectionString", "UseDevelopmentStorage=true" },
                    { "ConfigNames", "SFA.DAS.EmployerIncentives" }
                };

            _testContext = testContext;            

            _host = new HostBuilder()
                .ConfigureAppConfiguration(a =>
                    {
                        a.Sources.Clear();
                        a.AddInMemoryCollection(_appConfig);
                    })
                .ConfigureWebJobs(builder => builder
                       .AddHttp(options => options.SetResponse = (request, o) => LastResponse = o as HttpResponseMessage)
                       //.AddTimers()                         
                       .AddDurableTask(options =>
                       {
                           options.HubName = HubName;
                           options.UseAppLease = false;
                           options.UseGracefulShutdown = false;
                           options.ExtendedSessionsEnabled = false;
                           options.StorageProvider["maxQueuePollingInterval"] = new TimeSpan(0, 0, 0, 0, 500);
                           options.StorageProvider["partitionCount"] = 1;                           
                           options.NotificationUrl = new Uri("localhost:7071");
                           //options.StorageProvider["controlQueueBatchSize"] = 5;
                           //options.HttpSettings.DefaultAsyncRequestSleepTimeMilliseconds = 500;
                           //options.MaxConcurrentActivityFunctions = 10;
                           //options.MaxConcurrentOrchestratorFunctions = 5;
                       })
                       .AddAzureStorageCoreServices()
                       .ConfigureServices(s =>
                       {
                           new Startup().Configure(builder);

                           s.Configure<MatchedLearnerApi>(l =>
                           {
                               l.ApiBaseUrl = _testContext.LearnerMatchApi.BaseAddress;
                               l.Identifier = "";
                               l.Version = "1.0";
                           });

                           s.Configure<ApplicationSettings>(a =>
                           {
                               a.DbConnectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                               a.DistributedLockStorage = "UseDevelopmentStorage=true";
                               a.NServiceBusConnectionString = "UseLearningEndpoint=true";
                               a.UseLearningEndpointStorageDirectory = Path.Combine(testContext.TestDirectory.FullName, ".learningtransport");
                           });
                       })
                       )
                    .ConfigureServices(s =>
                    {
                        s.AddHostedService<PurgeBackgroundJob>();
                    })
                .Build();
        }

        public async Task StartHost()
        {
            await Task.WhenAll(_host.StartAsync(), Jobs.Terminate());
        }

        public Task Start(OrchestrationStarterInfo starter)
        {
            return Jobs.Start(starter);
        }

        public async Task DisposeAsync()
        {
            await Jobs.StopAsync();
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                _host.Dispose();
            }

            isDisposed = true;
        }
    }
}
