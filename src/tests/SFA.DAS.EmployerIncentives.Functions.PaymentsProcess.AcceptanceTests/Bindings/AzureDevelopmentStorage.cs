using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Bindings
{
    [Binding]
    public static class AzureDevelopmentStorage
    {

        [BeforeTestRun(Order = 0)]
        public static async Task CleanUpBefore()
        {
            await CleanUpTables();
            await CleanUpQueues();
            await CleanUpBlobs();
        }

        [AfterTestRun(Order = 100)]
        public static async Task CleanUpAfter()
        {
            await CleanUpTables();
            await CleanUpQueues();
            await CleanUpBlobs();
        }

        public static async Task CleanUpTables(string startsWith = TestContext.TestPrefix)
        {
            try
            {
                var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");

                foreach (var table in tableServiceClient.Query())
                {
                    if (table.Name.StartsWith(startsWith))
                    {
                        await tableServiceClient.DeleteTableAsync(table.Name);
                    }
                }
            }
            catch (Exception) { }
        }

        public static async Task CleanUpQueues(string startsWith = TestContext.TestPrefix)
        {
            try
            {
                var queueClient = new QueueServiceClient("UseDevelopmentStorage=true");

                foreach (var queue in queueClient.GetQueues())
                {
                    if (queue.Name.StartsWith(startsWith.ToLower()))
                    {
                        await queueClient.DeleteQueueAsync(queue.Name);
                    }
                }
            }
            catch (Exception) { }
        }

        public static async Task CleanUpBlobs(string startsWith = TestContext.TestPrefix)
        {
            try
            {
                var blobClient = new BlobServiceClient("UseDevelopmentStorage=true");

                foreach (var container in blobClient.GetBlobContainers())
                {
                    if (container.Name.StartsWith(startsWith.ToLower()))
                    {
                        await blobClient.DeleteBlobContainerAsync(container.Name);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
