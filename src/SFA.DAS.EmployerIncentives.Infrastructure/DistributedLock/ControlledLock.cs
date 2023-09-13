using Azure.Storage.Blobs.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    [ExcludeFromCodeCoverage]
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
