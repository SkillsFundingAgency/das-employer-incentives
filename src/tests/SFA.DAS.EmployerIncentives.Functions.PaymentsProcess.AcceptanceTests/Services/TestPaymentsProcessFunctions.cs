﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestPaymentsProcessFunctions : IDisposable
    {
        private const string TestConfigFile = "local.settings.json";
        private readonly TestContext _testContext;
        private const int Port = 7002;
        private readonly Settings _settings = new Settings();
        private readonly HttpClient _client;
        private bool _isDisposed;
        private Process _functionHostProcess;

        public TestPaymentsProcessFunctions(TestContext testContext)
        {
            _testContext = testContext;
            _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}") };
        }

        public async Task Start()
        {
            await ReplaceDbConnectionString();
            ReadTestConfig();
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
                orchestrationResponse = await _client.GetAsync(url);
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
                var statusResponse = await _client.GetAsync(azureFunctionOrchestrationLinks.StatusQueryGetUri);
                statusResponse.EnsureSuccessStatusCode();
                var statusJson = await statusResponse.Content.ReadAsStringAsync();
                azureFunctionOrchestrationStatus = JsonConvert.DeserializeObject<AzureFunctionOrchestrationStatus>(statusJson);

                return azureFunctionOrchestrationStatus.RuntimeStatus == "Pending" || azureFunctionOrchestrationStatus.RuntimeStatus == "Running";
            });
            return azureFunctionOrchestrationStatus;
        }

        public void StartFunctionHost()
        {
            var functionHostPath = Environment.ExpandEnvironmentVariables(_settings.FunctionHostPath);
            if (!File.Exists(functionHostPath)) throw new Exception("Wrong path to func.exe");

            var functionAppFolder = Path.Combine(
                    Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)), _settings.FunctionApplicationPath);
            if (!Directory.Exists(functionAppFolder)) throw new Exception("Wrong path to functions' bin folder");

            var functionConfig = Path.Combine(functionAppFolder, TestConfigFile);
            File.Copy(TestConfigFile, functionConfig, overwrite: true);

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k \"{functionHostPath}\" start -p {Port}",
                WorkingDirectory = functionAppFolder,
                UseShellExecute = true,
            };
            _functionHostProcess = new Process { StartInfo = startInfo };
            var success = _functionHostProcess.Start();

            if (!success) throw new InvalidOperationException("Could not start Azure Functions host.");
        }

        private void ReadTestConfig()
        {
            new ConfigurationBuilder()
                .AddJsonFile(TestConfigFile, optional: false) // Must have it!
                .AddEnvironmentVariables()
                .Build()
                .Bind("TestSettings", _settings);
        }

        private async Task ReplaceDbConnectionString()
        {
            var escapedConnString = HttpUtility.JavaScriptStringEncode(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await File.WriteAllTextAsync(TestConfigFile, (await File.ReadAllTextAsync(TestConfigFile))
                .Replace("DB_CONNECTION_STRING", escapedConnString));
        }
    }
}
