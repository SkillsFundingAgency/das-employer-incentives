using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class PausePaymentsRequest
    {
        public long ULN { get; set; }
        public long AccountLegalEntityId { get; set; }
        public PausePaymentsAction? Action { get; set; }
        public ServiceRequest ServiceRequest { get; set;  }
    }
}