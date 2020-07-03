namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class AddLegalEntityRequest
    {
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string OrganisationName { get; set; }
    }
}
