using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class ReinstatePaymentsRequest
    {
        public List<Guid> Payments { get; set; }
        public ReinstatePaymentsServiceRequest ServiceRequest { get; set;  }
    }

    public class ReinstatePaymentsServiceRequest
    {
        public string TaskId { get; set; }
        public string DecisionReference { get; set; }
        public DateTime TaskCreatedDate { get; set; }
        public string Process { get; set; }
    }
}