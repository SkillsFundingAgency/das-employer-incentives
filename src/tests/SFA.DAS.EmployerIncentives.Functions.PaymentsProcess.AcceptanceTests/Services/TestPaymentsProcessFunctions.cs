using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Transport;
using Polly;
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
        private readonly TestMessageBus _testMessageBus;
        private readonly List<IHook> _messageHooks;
        private IHost _host;
        private bool _isDisposed;
        private readonly ILogger _logger;
        private Process _functionHostProcess;
        private const int Port = 7002;
        protected readonly HttpClient Client;
        private Settings _settings;

        public TestPaymentsProcessFunctions(TestContext testContext)
        {
            _testContext = testContext;
            _testMessageBus = testContext.TestMessageBus;
            _messageHooks = testContext.Hooks;

            Client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };

            _hostConfig = new Dictionary<string, string>();

            _appConfig = new Dictionary<string, string>
            {
                {"EnvironmentName", "LOCAL_ACCEPTANCE_TESTS"},
                {"ConfigurationStorageConnectionString", "UseDevelopmentStorage=true"},
                {"ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true"},
                {"NServiceBusConnectionString", "UseDevelopmentStorage=true"},
                {"ConfigNames", "SFA.DAS.EmployerIncentives"}
            };

            _logger = new LoggerFactory().CreateLogger<TestPaymentsProcessFunctions>();
        }

        public async Task<AzureFunctionOrchestrationStatus> StartPaymentsProcess(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var orchestrationLinks = await StartFunctionOrchestration(collectionPeriodYear, collectionPeriodMonth);
            var status = await CompleteFunctionOrchestration(orchestrationLinks);

            return status;
        }

        private async Task<AzureFunctionOrchestrationStatus> CompleteFunctionOrchestration(AzureFunctionOrchestrationLinks azureFunctionOrchestrationLinks)
        {
            var policy = Policy
                .HandleResult<bool>(true)
                .WaitAndRetryAsync(new[]
                {
                    // Tweak these time-outs if you're still getting errors 👇
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                });

            AzureFunctionOrchestrationStatus azureFunctionOrchestrationStatus = null;
            await policy.ExecuteAsync(async () =>
            {
                var statusResponse = await Client.GetAsync(azureFunctionOrchestrationLinks.StatusQueryGetUri);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                azureFunctionOrchestrationStatus = JsonConvert.DeserializeObject<AzureFunctionOrchestrationStatus>(statusJson);

                return azureFunctionOrchestrationStatus.RuntimeStatus == "Pending" || azureFunctionOrchestrationStatus.RuntimeStatus == "Running";
            });
            return azureFunctionOrchestrationStatus;
        }

        private async Task<AzureFunctionOrchestrationLinks> StartFunctionOrchestration(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(new[]
                {
                    // Tweak these time-outs if you're still getting errors 👇
                    TimeSpan.FromMilliseconds(250),
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                });

            var url = $"api/orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriodMonth}";
            HttpResponseMessage orchestrationResponse = null;
            await policy.ExecuteAsync(async () =>
            {
                orchestrationResponse = await Client.GetAsync(url);
                orchestrationResponse.EnsureSuccessStatusCode();
            });

            var linksJson = await orchestrationResponse.Content.ReadAsStringAsync();
            var links = JsonConvert.DeserializeObject<AzureFunctionOrchestrationLinks>(linksJson);
            return links;
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
                        if (_testMessageBus != null)
                            a.SetBasePath(_testMessageBus.StorageDirectory.FullName);
                    })
                    .ConfigureWebJobs(startUp.Configure)
                ;

            if (_testMessageBus != null)
            {
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

                            if (_messageHooks.SingleOrDefault(h => h is Hook<MessageContext>) is Hook<MessageContext>
                                hook)
                            {
                                o.OnMessageReceived = (message) => { hook?.OnReceived(message); };
                                o.OnMessageProcessed = (message) => { hook?.OnProcessed(message); };
                                o.OnMessageErrored = (exception, message) => { hook?.OnErrored(exception, message); };
                            }
                        });
                });
            }

            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json")
                .AddEnvironmentVariables()
                .Build();
            _settings = new Settings();
            configurationRoot.Bind(_settings);

            hostBuilder.UseEnvironment("LOCAL");
            _host = await hostBuilder.StartAsync();

            StartFunctionHost();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _host?.StopAsync();
            }

            if (_functionHostProcess != null)
            {
                if (!_functionHostProcess.HasExited)
                {
                    _functionHostProcess.Kill();
                }

                _functionHostProcess.Dispose();
            }

            _isDisposed = true;
        }

        public void StartFunctionHost()
        {
            var dotnetExePath = Environment.ExpandEnvironmentVariables(_settings.DotnetExecutablePath);
            if (!File.Exists(dotnetExePath)) throw new Exception("Wrong path to dotnet.exe");

            var functionHostPath = Environment.ExpandEnvironmentVariables(_settings.FunctionHostPath);
            if (!File.Exists(functionHostPath)) throw new Exception("Wrong path to func.dll");

            var functionAppFolder = Path.GetRelativePath(Directory.GetCurrentDirectory(), _settings.FunctionApplicationPath);
            if (!Directory.Exists(functionAppFolder)) throw new Exception("Wrong path to functions' bin folder");

            _functionHostProcess = new Process
            {
                StartInfo =
                {
                    FileName =  dotnetExePath,
                    Arguments = $"\"{functionHostPath}\" start -p {Port}",
                    WorkingDirectory = functionAppFolder,
                }
            };
            var success = _functionHostProcess.Start();
            if (!success)
            {
                throw new InvalidOperationException("Could not start Azure Functions host.");
            }
        }


        public class Settings
        {
            public string DotnetExecutablePath { get; set; }
            public string FunctionHostPath { get; set; }
            public string FunctionApplicationPath { get; set; }
        }
    }

}
