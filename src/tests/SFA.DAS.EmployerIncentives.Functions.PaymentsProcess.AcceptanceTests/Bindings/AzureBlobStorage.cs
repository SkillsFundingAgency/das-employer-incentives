using Azure.Storage.Blobs;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "azureBlobStorage")]
    public class AzureBlobStorage
    {
        [BeforeScenario()]
        public async Task CreateStorage(TestContext context)
        {
            var container = new BlobContainerClient("UseDevelopmentStorage=true", context.InstanceId);
            await container.CreateAsync();

            context.BlobClient = container;
        }

        [AfterScenario()]
        public async Task DeleteStorage(TestContext context)
        {
            await context.BlobClient?.DeleteAsync();
        }
    }
}
