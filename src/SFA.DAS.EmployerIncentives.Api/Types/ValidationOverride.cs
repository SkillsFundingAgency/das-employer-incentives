namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class ValidationOverride
    {
        public long AccountLegalEntityId { get; set; }
        public long ULN { get; set; }
        public ValidationStep[] ValidationSteps { get; set; }
        public ServiceRequest ServiceRequest { get; set; }
    }
}
