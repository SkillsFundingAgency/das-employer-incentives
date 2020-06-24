using Microsoft.WindowsAzure.Storage.Blob;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    public class ControlledLock
    {
        public string Id { get; set; }
        public string LeaseId { get; set; }
        public CloudBlockBlob Blob { get; set; }

        public ControlledLock(string id, string leaseId, CloudBlockBlob blob)
        {
            Id = id;
            LeaseId = leaseId;
            Blob = blob;
        }
    }
}
