namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries
{
    public class LegalEntityVendorRegistrationFormDto
    {
        public long LegalEntityId { get; set; }
        public string VrfCaseId { get; set; }
        public string VrfVendorId { get; set; }
        public string VrfCaseStatus { get; set; }
    }
}
