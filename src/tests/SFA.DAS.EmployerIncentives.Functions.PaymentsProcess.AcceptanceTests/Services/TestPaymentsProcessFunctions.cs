using Newtonsoft.Json;
using Polly;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private const int Port = 7003;
        private const int StartupTimeoutInSeconds = 15;
        private const int OrchestratorRunTimeoutInSeconds = 30;

        private bool _isDisposed;
        private readonly HttpClient _client;
        private readonly FunctionsController _functionsController;
        private readonly TestPaymentsProcessFunctionsConfigurator _configurator;
        private DateTime _functionHostStartedOn;

        public TestPaymentsProcessFunctions(TestContext context)
        {
            _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };
            _functionsController = new FunctionsController();
            _configurator = new TestPaymentsProcessFunctionsConfigurator(context.SqlDatabase.DatabaseInfo.ConnectionString,
                context.LearnerMatchApi.BaseAddress);
        }

        public async Task Start()
        {
            _functionsController.StartFunctionsHost(Port, _configurator.Setup().Settings);
            await FunctionsOrchestratorIsReady();
            _functionHostStartedOn = DateTime.UtcNow;
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
            _client.Dispose();
            _isDisposed = true;
        }

        public async Task<OrchestrationStatus> StartPaymentsProcess(short collectionPeriodYear, byte collectionPeriodNumber)
        {
            var orchestrationLinks = await StartIncentivePaymentOrchestrator(collectionPeriodYear, collectionPeriodNumber);
            return await FunctionOrchestrationCompleted(orchestrationLinks);
        }

        public async Task StartLearnerMatching()
        {
            await StartLearnerMatchingOrchestrator();
            Thread.Sleep(TimeSpan.FromSeconds(1)); // time it takes function host to update storage 🤷‍
            await AllFunctionOrchestrationCompleted();
        }

        private async Task<OrchestrationLinks> StartIncentivePaymentOrchestrator(short collectionPeriodYear, byte collectionPeriodNumber)
        {
            var url = $"api/orchestrators/IncentivePaymentOrchestrator/{collectionPeriodYear}/{collectionPeriodNumber}";
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
        }

        public async Task AllFunctionOrchestrationCompleted()
        {
            var policy = Policy
                .HandleResult(true)
                .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), OrchestratorRunTimeoutInSeconds));

            const string statusQuery = "Running,Pending";

            var url = $"runtime/webhooks/durabletask/instances?createdTimeFrom={_functionHostStartedOn:s}&runtimeStatus={statusQuery}";
            List<OrchestrationStatus> status;

            await policy.ExecuteAsync(async () =>
            {
                var statusResponse = await _client.GetAsync(url);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                status = JsonConvert.DeserializeObject<List<OrchestrationStatus>>(statusJson);

                Console.WriteLine($"[{nameof(TestPaymentsProcessFunctions)}] AllFunctionOrchestrationCompleted: {status.Count} functions in status {statusQuery}");
                return status.Any();
            });
        }

        private async Task FunctionsOrchestratorIsReady()
        {
            try
            {
                var url = "runtime/webhooks/durabletask/instances";

                var policy = Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(Enumerable.Repeat(TimeSpan.FromSeconds(1), StartupTimeoutInSeconds));

                await policy.ExecuteAsync(async () =>
                {
                    Console.WriteLine($"[{nameof(TestPaymentsProcessFunctions)}] FunctionsOrchestratorIsReady");
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
                Console.WriteLine($"[{nameof(TestPaymentsProcessFunctions)}] FunctionOrchestrationCompleted");
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
