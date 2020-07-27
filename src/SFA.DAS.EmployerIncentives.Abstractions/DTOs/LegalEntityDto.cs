namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs
{
    public class LegalEntityDto
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
    }
}