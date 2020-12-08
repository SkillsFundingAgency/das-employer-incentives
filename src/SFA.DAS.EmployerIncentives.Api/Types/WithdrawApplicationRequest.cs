using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class WithdrawApplicationRequest
    {
        public WithdrawlType WithdrawlType { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }
        public string ServiceRequestTaskId { get; set; }
        public string ServiceRequestDecisionNumber { get; set; }
        public DateTime ServiceRequestCreatedDate { get; set; }
    }
}
