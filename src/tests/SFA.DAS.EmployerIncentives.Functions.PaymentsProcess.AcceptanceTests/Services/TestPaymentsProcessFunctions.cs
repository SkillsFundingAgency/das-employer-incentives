using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private readonly TestContext _testContext;
        private readonly Dictionary<string, string> _appConfig;
        private readonly Dictionary<string, string> _hostConfig;
        private readonly Dictionary<string, string> _testConfig;
        private readonly TestMessageBus _testMessageBus;
        private readonly List<IHook> _messageHooks;
        private IHost host;
        private bool isDisposed;
        private ILogger _logger;
        private Process _funcHostProcess;
        public int Port { get; } = 7001;
        public readonly HttpClient Client = new HttpClient();

        public TestPaymentsProcessFunctions(TestContext testContext)
        {
            _testContext = testContext;
            _testMessageBus = testContext.TestMessageBus;
            _messageHooks = testContext.Hooks;

            _testConfig = new Dictionary<string, string>
            {
                {"DotnetExecutablePath", @"%ProgramFiles%\dotnet\dotnet.exe"},
                {"FunctionHostPath", @"%APPDATA%\npm\node_modules\azure-functions-core-tools\bin\func.dll"},
                {
                    "FunctionApplicationPath",
                    @"..\..\..\..\..\SFA.DAS.EmployerIncentives.Functions.PaymentsProcess\bin\Debug\netcoreapp3.1"
                },
            };

            _hostConfig = new Dictionary<string, string>();

            _appConfig = new Dictionary<string, string>
            {
                {"EnvironmentName", "LOCAL_ACCEPTANCE_TESTS"},
                {"ConfigurationStorageConnectionString", "UseDevelopmentStorage=true"},
                // { "ConfigNames", "SFA.DAS.EmployerIncentives.PaymentsProcess" },
                {"NServiceBusConnectionString", "UseDevelopmentStorage=true"},
                {"AzureWebJobsStorage", "UseDevelopmentStorage=true"}
            };

            _logger = new LoggerFactory().CreateLogger<TestPaymentsProcessFunctions>();
        }

        public async Task<HttpResponseMessage> StartPaymentsProcess(short collectionPeriodYear,
            byte collectionPeriodMonth)
        {
            var url = $"orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriodMonth}";
            return await Client.GetAsync(url);

            //var request = new HttpRequestMessage(HttpMethod.Get, url);

            //return await IncentivePaymentOrchestratorHttpStart.HttpStart(request, null, collectionPeriodYear,
            //    collectionPeriodMonth, _logger);
        }

        public async Task Start()
        {
            var startUp = new Startup();

            var hostBuilder = new HostBuilder()
                    .ConfigureHostConfiguration(a =>
                    {
                        a.Sources.Clear();
                        a.AddInMemoryCollection(_hostConfig);
                    })
                    .ConfigureAppConfiguration(a =>
                    {
                        a.Sources.Clear();
                        a.AddInMemoryCollection(_appConfig);
                        a.SetBasePath(_testMessageBus.StorageDirectory.FullName);
                    })
                    .ConfigureWebJobs(startUp.Configure)
                ;

            _ = hostBuilder.ConfigureServices((s) =>
            {
                _ = s.AddNServiceBus(_logger,
                    (o) =>
                    {
                        o.EndpointConfiguration = (endpoint) =>
                        {
                            endpoint.UseTransport<LearningTransport>()
                                .StorageDirectory(_testMessageBus.StorageDirectory.FullName);
                            return endpoint;
                        };

                        var hook =
                            _messageHooks.SingleOrDefault(h => h is Hook<MessageContext>) as Hook<MessageContext>;
                        if (hook != null)
                        {
                            o.OnMessageReceived = (message) => { hook?.OnReceived(message); };
                            o.OnMessageProcessed = (message) => { hook?.OnProcessed(message); };
                            o.OnMessageErrored = (exception, message) => { hook?.OnErrored(exception, message); };
                        }
                    });
            });

            hostBuilder.UseEnvironment("LOCAL");
            host = await hostBuilder.StartAsync();

            StartFunctionHost();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing && host != null)
            {
                host.StopAsync();
            }

            isDisposed = true;
        }



        public void StartFunctionHost()
        {
            var dotnetExePath = Environment.ExpandEnvironmentVariables(_testConfig["DotnetExecutablePath"]);
            if (!File.Exists(dotnetExePath)) throw new Exception("Wrong path to dotnet.exe");

            var functionHostPath = Environment.ExpandEnvironmentVariables(_testConfig["FunctionHostPath"]);
            if (!File.Exists(functionHostPath)) throw new Exception("Wrong path to func.dll");

            var functionAppFolder =
                Path.GetRelativePath(Directory.GetCurrentDirectory(), _testConfig["FunctionApplicationPath"]);
            if (!Directory.Exists(functionAppFolder)) throw new Exception("Wrong path to function's bin folder");

            _funcHostProcess = new Process
            {
                StartInfo =
                {
                    FileName = dotnetExePath,
                    Arguments = $"\"{functionHostPath}\" start -p {Port}",
                    WorkingDirectory = functionAppFolder
                }
            };
            var success = _funcHostProcess.Start();
            if (!success)
            {
                throw new InvalidOperationException("Could not start Azure Functions host.");
            }

            Client.BaseAddress = new Uri($"http://localhost:{Port}");
        }


    }

}
