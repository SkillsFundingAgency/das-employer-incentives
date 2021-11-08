namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class SignAgreementRequest
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public long LegalEntityId { get; set; }
        public int AgreementVersion { get; set; }
    }
}
