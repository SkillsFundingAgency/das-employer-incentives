using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class PausePaymentsRequest
    {
        public PausePaymentsAction? Action { get; set; }
        public Application[] Applications { get; set; }
        public ServiceRequest ServiceRequest { get; set;  }
    }
}