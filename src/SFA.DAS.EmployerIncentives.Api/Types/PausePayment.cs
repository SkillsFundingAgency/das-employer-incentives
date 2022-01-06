using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class PausePayment
    {
        public long ULN { get; set; }
        public long AccountLegalEntityId { get; set; }
        public PausePaymentsAction? Action { get; set; }
        public ServiceRequest ServiceRequest { get; set;  }
    }
}