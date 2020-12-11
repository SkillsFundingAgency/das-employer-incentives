using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PausePaymentsRequest
    {
        public string ServiceRequestId { get; set;  }
        public string DecisionReferenceNumber { get; set; }
        public DateTime DateServiceRequestTaskCreated { get; set; }
    }
}