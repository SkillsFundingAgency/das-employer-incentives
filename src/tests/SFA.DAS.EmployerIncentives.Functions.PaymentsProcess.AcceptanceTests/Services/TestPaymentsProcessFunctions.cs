using Newtonsoft.Json;
using Polly;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private const int Port = 7007;
        private const int StartupTimeoutInSeconds = 30;
        private const int OrchestratorRunTimeoutInSeconds = 10;

        private bool _isDisposed;
        private readonly HttpClient _client;
        private readonly FunctionsController _functionsController;
        private readonly TestPaymentsProcessFunctionsConfigurator _configurator;

        public TestPaymentsProcessFunctions(string databaseConnectionString, string learnerMatchApiBaseUrl)
        {
            _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };
            _functionsController = new FunctionsController();
            _configurator = new TestPaymentsProcessFunctionsConfigurator(databaseConnectionString, learnerMatchApiBaseUrl);
        }

        public async Task Start()
        {
            _functionsController.StartFunctionsHost(Port, _configurator.Setup().Settings);
            await FunctionsOrchestratorIsReady();
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
            _isDisposed = true;
        }

        public async Task<OrchestrationStatus> StartPaymentsProcess(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var orchestrationLinks = await StartIncentivePaymentOrchestrator(collectionPeriodYear, collectionPeriodMonth);
            return await FunctionOrchestrationCompleted(orchestrationLinks);
        }

        public async Task StartLearnerMatching()
        {
            await AllFunctionOrchestrationCompleted();
            await StartLearnerMatchingOrchestrator();
            await AllFunctionOrchestrationCompleted();
        }

        private async Task<OrchestrationLinks> StartIncentivePaymentOrchestrator(short collectionPeriodYear, byte collectionPeriod)
        {
            var url = $"api/orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriod}";
            var orchestrationResponse = await _client.GetAsync(url);
            orchestrationResponse.EnsureSuccessStatusCode();
            var json = await orchestrationResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrchestrationLinks>(json);
        }

        private async Task StartLearnerMatchingOrchestrator()
        {
            var url = $"admin/functions/LearnerMatchingOrchestrator_Start";
            var content = new StringContent("{ input : null }", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
        }

        public async Task AllFunctionOrchestrationCompleted()
        {
            var policy = Policy
                .HandleResult(true)
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), StartupTimeoutInSeconds));

            var url = $"http://localhost:{Port}/runtime/webhooks/durabletask/instances?&runtimeStatus=Running,Pending";
            List<OrchestrationStatus> status;

            await policy.ExecuteAsync(async () =>
            {
                Console.WriteLine("[TestPaymentsProcessFunctions] AllFunctionOrchestrationCompleted");
                var statusResponse = await _client.GetAsync(url);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<OrchestrationStatus>>(statusJson);
                return status.Any();
            });
        }

        private async Task FunctionsOrchestratorIsReady()
        {
            try
            {
                var url = $"http://localhost:{Port}/runtime/webhooks/durabletask/instances";

                var policy = Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), StartupTimeoutInSeconds));

                await policy.ExecuteAsync(async () =>
                {
                    Console.WriteLine("[TestPaymentsProcessFunctions] FunctionsOrchestratorIsReady");
                    var statusResponse = await _client.GetAsync(url);
                    statusResponse.EnsureSuccessStatusCode();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start functions host. {ex.Message}");
                throw;
            }
        }

        private async Task<OrchestrationStatus> FunctionOrchestrationCompleted(OrchestrationLinks orchestrationLinks)
        {
            var policy = Policy
                .HandleResult(true)
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), OrchestratorRunTimeoutInSeconds));

            OrchestrationStatus orchestrationStatus = null;
            await policy.ExecuteAsync(async () =>
            {
                Console.WriteLine("[TestPaymentsProcessFunctions] FunctionOrchestrationCompleted");
                var statusResponse = await _client.GetAsync(orchestrationLinks.StatusQueryGetUri);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                orchestrationStatus = JsonConvert.DeserializeObject<OrchestrationStatus>(statusJson);

                return orchestrationStatus.RuntimeStatus == "Pending" || orchestrationStatus.RuntimeStatus == "Running";
            });
            return orchestrationStatus;
        }

    }
}
