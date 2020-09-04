namespace SFA.DAS.EmployerIncentives.Messages.Events
{
    public class UpdateLegalEntityVrfCaseStatusEvent
    {
        public long LegalEntityId { get; set; }
        public string VrfCaseId { get; set; }
    }
}
