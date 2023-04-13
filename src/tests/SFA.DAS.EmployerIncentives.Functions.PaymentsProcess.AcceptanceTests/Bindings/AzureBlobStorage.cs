using Azure.Storage.Blobs;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public class AzureBlobStorage
    {
        [BeforeScenario()]
        public async Task CreateStorage(TestContext context)
        {
            var container = new BlobContainerClient("UseDevelopmentStorage=true", $"{TestContext.TestPrefix}{context.InstanceId}".ToLower());
            await container.CreateAsync();

            context.BlobClient = container;
        }
    }
}
