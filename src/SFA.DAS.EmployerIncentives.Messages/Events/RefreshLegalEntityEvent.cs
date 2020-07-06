namespace SFA.DAS.EmployerIncentives.Messages.Events
{
    public class RefreshLegalEntityEvent
    { 
        public long AccountId { get; set; }
        public long LegalEntityId { get; set; }
        public string OrganisationName { get; set; }
        public long AccountLegalEntityId { get; set; }
    }
}
