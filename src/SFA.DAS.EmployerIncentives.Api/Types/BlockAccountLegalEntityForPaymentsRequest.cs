using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class BlockAccountLegalEntityForPaymentsRequest
    {
        public List<VendorBlock> VendorBlocks { get; set; }
        public ServiceRequest ServiceRequest { get; set; }
    }
}
