using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Extensions
{
    public static class BlobContainerClientExtensions
    {
        public static int ItemCount(this BlobContainerClient client)
        {
            int itemCount = 0;
            foreach (var blobItem in client.GetBlobs())
            {
                itemCount++;
            }
            return itemCount;
        }
    }
}
