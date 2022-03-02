using DurableTask.AzureStorage;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class FunctionsHost
    {
        private readonly TestContext _testContext;
        private readonly FeatureContext _featureContext;
        public FunctionsHost(TestContext testContext, FeatureContext featureContext)
        {
            _testContext = testContext;
            _featureContext = featureContext;
        }

        [BeforeScenario()]
        public async Task InitialiseHost()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _testContext.HubName = Truncate($"EITEST{GenerateId()}{_featureContext.FeatureInfo.Title}", 44);
            _testContext.TestFunction = new TestFunction(_testContext);
            try
            {
                await _testContext.TestFunction.StartHost();
            }
            catch(InvalidOperationException ex)
            {
                if (ex.Message != "Server has already started")
                {
                    throw;
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.TestFunction.HubName}");
        }

        [AfterScenario()]
        public async Task Cleanup()
        { 
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (_testContext.TestFunction != null)
            {                
                await _testContext.TestFunction.Terminate();
                await _testContext.TestFunction.DisposeAsync();
                _testContext.TestFunction = null;
            }
            await DeleteTaskHubAsync();

            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {_testContext.HubName}");
        }

        private async Task DeleteTaskHubAsync()
        {
            if (_testContext.HubName == null) return;

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                StorageConnectionString = "UseDevelopmentStorage=true",
                TaskHubName = _testContext.HubName
            };

            var service = new AzureStorageOrchestrationService(settings);
            await service.DeleteAsync();
        }

        private static string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= (b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
