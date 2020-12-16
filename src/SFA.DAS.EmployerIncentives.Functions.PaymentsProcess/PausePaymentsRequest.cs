using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class PausePaymentsRequest
    {
        public long ULN { get; set; }
        public PausePaymentsAction? Action { get; set; }
        public string ServiceRequestId { get; set;  }
        public string DecisionReferenceNumber { get; set; }
        public DateTime DateServiceRequestTaskCreated { get; set; }
    }
}