using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using Polly;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private const string TestConfigFile = "local.settings.json";
        private const bool HideFunctionHostWindow = false; // Set to `false` if debugging

        private readonly TestContext _testContext;
        private readonly Dictionary<string, string> _appConfig;
        private readonly Dictionary<string, string> _hostConfig;
        private readonly TestMessageBus _testMessageBus;
        private readonly List<IHook> _messageHooks;
        private IHost _host;
        private bool _isDisposed;
        private readonly ILogger _logger;
        private Process _functionHostProcess;
        private const int Port = 7001;
        protected readonly HttpClient Client;
        private readonly Settings _settings = new Settings();

        public TestPaymentsProcessFunctions(TestContext testContext)
        {
            _testContext = testContext;
            _testMessageBus = testContext.TestMessageBus;
            _messageHooks = testContext.Hooks;

            Client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };

            _logger = new LoggerFactory().CreateLogger<TestPaymentsProcessFunctions>();
        }


        public async Task Start()
        {
            var escapedConnString = HttpUtility.JavaScriptStringEncode(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await File.WriteAllTextAsync(TestConfigFile, (await File.ReadAllTextAsync(TestConfigFile))
                .Replace("DB_CONNECTION_STRING", escapedConnString));

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

            if (_functionHostProcess != null)
            {
                if (!_functionHostProcess.HasExited)
                {
                    _functionHostProcess.Kill();
                }

                _functionHostProcess.CloseMainWindow();
                _functionHostProcess.Dispose();
            }

            _testContext.SqlDatabase?.Dispose();
            _isDisposed = true;
        }

        public async Task<AzureFunctionOrchestrationStatus> StartPaymentsProcess(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var orchestrationLinks = await StartFunctionOrchestration(collectionPeriodYear, collectionPeriodMonth);
            return await CompleteFunctionOrchestration(orchestrationLinks);
        }

        private async Task<AzureFunctionOrchestrationLinks> StartFunctionOrchestration(short collectionPeriodYear, byte collectionPeriod)
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(new[]
                {
                    // Tweak these time-outs if you're still getting errors 👇
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                });

            var url = $"api/orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriod}";
            HttpResponseMessage orchestrationResponse = null;
            await policy.ExecuteAsync(async () =>
            {
                orchestrationResponse = await Client.GetAsync(url);
                orchestrationResponse.EnsureSuccessStatusCode();
            });

            var linksJson = await orchestrationResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AzureFunctionOrchestrationLinks>(linksJson);
        }

        private async Task<AzureFunctionOrchestrationStatus> CompleteFunctionOrchestration(AzureFunctionOrchestrationLinks azureFunctionOrchestrationLinks)
        {
            var policy = Policy
                .HandleResult<bool>(true)
                .WaitAndRetryAsync(new[]
                {
                    // Tweak these time-outs if you're still getting errors 👇
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
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

        public void StartFunctionHost()
        {
            new ConfigurationBuilder()
                .AddJsonFile(TestConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("TestSettings", _settings);

            //var dotnetExePath = Environment.ExpandEnvironmentVariables(_settings.DotnetExecutablePath);
            //if (!File.Exists(dotnetExePath)) throw new Exception("Wrong path to dotnet.exe");

            var functionHostPath = Environment.ExpandEnvironmentVariables(_settings.FunctionHostPath);
            if (!File.Exists(functionHostPath)) throw new Exception("Wrong path to func.dll");

            var functionAppFolder = Path.Combine(
                    Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)), _settings.FunctionApplicationPath);
            if (!Directory.Exists(functionAppFolder)) throw new Exception("Wrong path to functions' bin folder");

            var functionConfig = Path.Combine(functionAppFolder, TestConfigFile);
            File.Copy(TestConfigFile, functionConfig);

            _functionHostProcess = new Process
            {
                StartInfo =
                {
                    FileName =  "cmd.exe",// dotnetExePath,
                    Arguments = $"/k \"{functionHostPath}\" start -p {Port}",
                    WorkingDirectory = functionAppFolder,
                    //RedirectStandardError = true,
                    //RedirectStandardOutput = true,
                  //  RedirectStandardInput = true,
                    UseShellExecute = true,
                    //CreateNoWindow = false,
                   // WindowStyle = ProcessWindowStyle.Normal,

                }
            };
            var success = _functionHostProcess.Start();

            if (!success)
            {
                throw new InvalidOperationException("Could not start Azure Functions host.");
            }

            //   SaveFunctionHostProcessOutput();

        }

        public StringBuilder FunctionLogs { get; set; } = new StringBuilder();

        public void SaveFunctionHostProcessOutput()
        {
            bool errored = false;
            FunctionLogs.AppendLine("ERRORS:");
            while (!_functionHostProcess.StandardError.EndOfStream)
            {
                FunctionLogs.AppendLine(_functionHostProcess.StandardError.ReadLine());
                errored = true;
            }

            FunctionLogs.AppendLine("INFORMATION:");
            while (!_functionHostProcess.StandardOutput.EndOfStream)
            {
                FunctionLogs.AppendLine(_functionHostProcess.StandardOutput.ReadLine());
            }

            if (errored)
            {
                Assert.Fail(FunctionLogs.ToString());
            }
        }
    }
}
