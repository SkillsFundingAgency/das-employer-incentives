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
        [BeforeScenario()]
        public async Task InitialiseHost(TestContext testContext, FeatureContext featureContext)
        {
            Console.WriteLine($"Here !!!");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            testContext.HubName = Truncate($"EITEST{GenerateId()}{featureContext.FeatureInfo.Title}", 44);
            testContext.TestFunction = new TestFunction(testContext);
            try
            {
                await testContext.TestFunction.StartHost();
            }
            catch(InvalidOperationException ex)
            {
                if (ex.Message != "Server has already started")
                {
                    throw;
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Time it took to spin up Azure Functions Host: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {testContext.TestFunction.HubName}");
        }

        [AfterScenario()]
        public async Task Cleanup(TestContext testContext)
        { 
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (testContext.TestFunction != null)
            {                
                await testContext.TestFunction.Terminate();
                await testContext.TestFunction.DisposeAsync();
                testContext.TestFunction = null;
            }
            await DeleteTaskHubAsync(testContext);

            stopwatch.Stop();
            Console.WriteLine($"Time it took to Cleanup  FunctionsHost: {stopwatch.Elapsed.Milliseconds} milliseconds for hub {testContext.HubName}");
        }

        private static async Task DeleteTaskHubAsync(TestContext testContext)
        {
            if (testContext.HubName == null) return;

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                StorageConnectionString = "UseDevelopmentStorage=true",
                TaskHubName = testContext.HubName
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
