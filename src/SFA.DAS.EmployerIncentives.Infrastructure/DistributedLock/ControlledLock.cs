using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    public class ControlledLock
    {
        public string Id { get; set; }        
        public BlobLeaseClient Client { get; set; }

        public ControlledLock(string id, BlobLeaseClient client)
        {
            Id = id;
            Client = client;
        }
    }
}
