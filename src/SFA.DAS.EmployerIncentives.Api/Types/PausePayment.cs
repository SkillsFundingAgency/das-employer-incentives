using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class PausePayment
    {
        public PausePaymentsAction? Action { get; set; }
        public Application[] Applications { get; set; }
        public ServiceRequest ServiceRequest { get; set;  }
    }
}