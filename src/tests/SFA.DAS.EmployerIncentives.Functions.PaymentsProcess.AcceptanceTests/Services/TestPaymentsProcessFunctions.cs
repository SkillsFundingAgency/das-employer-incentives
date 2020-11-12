using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private const string TestConfigFile = "local.settings.json";
        private const int Port = 7007;
        private readonly TestContext _testContext;
        private readonly TestSettings _settings = new TestSettings();
        private readonly HttpClient _client;
        private readonly FunctionsController _functionsController;
        private bool _isDisposed;

        public TestPaymentsProcessFunctions(TestContext testContext)
        {
            _testContext = testContext;
            _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };
            _functionsController = new FunctionsController();
        }

        public async Task Start()
        {
            ReadSettings();

            var funcHost = Environment.ExpandEnvironmentVariables(_settings.FunctionHostPath);
            if (!File.Exists(funcHost)) throw new Exception("Wrong path to func.exe");

            var functionAppFolder = Path.Combine(
                Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)), _settings.FunctionApplicationPath);
            if (!Directory.Exists(functionAppFolder)) throw new Exception("Wrong path to functions' bin folder");

            var functionConfig = Path.Combine(functionAppFolder, TestConfigFile);
            File.Copy(TestConfigFile, functionConfig, overwrite: true);

            ReplaceDbConnectionString(functionConfig);
            ReplaceLearnerMatchUrlString(functionConfig);

            await _functionsController.StartFunctionsHost(Port, funcHost, functionAppFolder);
            await FunctionsHostAvailable();
        }

        private void ReadSettings()
        {
            new ConfigurationBuilder()
                .AddJsonFile(TestConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("TestSettings", _settings);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            _functionsController.Dispose();
            _testContext.SqlDatabase?.Dispose();
            _isDisposed = true;
        }

        private void ReplaceDbConnectionString(string pathToConfig)
        {
            var escapedConnString = HttpUtility.JavaScriptStringEncode(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            File.WriteAllText(pathToConfig, (File.ReadAllText(pathToConfig)).Replace("DB_CONNECTION_STRING", escapedConnString));
        }

        private void ReplaceLearnerMatchUrlString(string pathToConfig)
        {
            var baseAddress = HttpUtility.JavaScriptStringEncode(_testContext.LearnerMatchApi.BaseAddress);
            File.WriteAllText(pathToConfig, (File.ReadAllText(pathToConfig)).Replace("LEARNER_MATCH_API_URL", baseAddress));
        }

        public async Task<AzureFunctionOrchestrationStatus> StartPaymentsProcess(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var orchestrationLinks = await StartIncentivePaymentOrchestrator(collectionPeriodYear, collectionPeriodMonth);
            return await FunctionOrchestrationCompleted(orchestrationLinks);
        }

        public async Task StartLearnerMatching()
        {
            await StartLearnerMatchingOrchestrator();
            await AllFunctionOrchestrationCompleted();
        }

        private async Task<AzureFunctionOrchestrationLinks> StartIncentivePaymentOrchestrator(short collectionPeriodYear, byte collectionPeriod)
        {
            var url = $"api/orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriod}";
            var orchestrationResponse = await _client.GetAsync(url);
            orchestrationResponse.EnsureSuccessStatusCode();
            var json = await orchestrationResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AzureFunctionOrchestrationLinks>(json);
        }

        private async Task StartLearnerMatchingOrchestrator()
        {
            var url = $"admin/functions/LearnerMatchingOrchestrator_Start";
            var content = new StringContent("{ input : null }", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }

        private async Task AllFunctionOrchestrationCompleted()
        {
            var policy = Policy
                .HandleResult(true)
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), 10));

            var url = $"http://localhost:{Port}/runtime/webhooks/durabletask/instances?&runtimeStatus=Running,Pending";
            List<AzureFunctionOrchestrationStatus> status;

            await policy.ExecuteAsync(async () =>
            {
                var statusResponse = await _client.GetAsync(url);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<AzureFunctionOrchestrationStatus>>(statusJson);
                return status.Any();
            });
        }

        private async Task FunctionsHostAvailable()
        {
            var policy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), 10));

            var url = $"http://localhost:{Port}/runtime/webhooks/durabletask/instances";

            await policy.ExecuteAsync(async () =>
            {
                var statusResponse = await _client.GetAsync(url);
                statusResponse.EnsureSuccessStatusCode();
            });
        }

        private async Task<AzureFunctionOrchestrationStatus> FunctionOrchestrationCompleted(AzureFunctionOrchestrationLinks azureFunctionOrchestrationLinks)
        {
            var policy = Policy
                .HandleResult(true)
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), 10));

            AzureFunctionOrchestrationStatus azureFunctionOrchestrationStatus = null;
            await policy.ExecuteAsync(async () =>
            {
                var statusResponse = await _client.GetAsync(azureFunctionOrchestrationLinks.StatusQueryGetUri);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                azureFunctionOrchestrationStatus = JsonConvert.DeserializeObject<AzureFunctionOrchestrationStatus>(statusJson);

                return azureFunctionOrchestrationStatus.RuntimeStatus == "Pending" || azureFunctionOrchestrationStatus.RuntimeStatus == "Running";
            });
            return azureFunctionOrchestrationStatus;
        }

    }
}
