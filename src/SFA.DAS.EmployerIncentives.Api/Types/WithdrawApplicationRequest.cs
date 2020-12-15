using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class WithdrawApplicationRequest
    {
        public WithdrawalType WithdrawalType { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }
        public ServiceRequest ServiceRequest { get; set; }
    }

    public class ServiceRequest
    {
        public string TaskId { get; set; }
        public string DecisionReference { get; set; }
        public DateTime? TaskCreatedDate { get; set; }
    }
}
