namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateVendorRegistrationFormRequest
    {
        public long LegalEntityId { get; set; }
        public string CaseId { get; set; }
        public string VendorId { get; set; }
        public string Status { get; set; }
    }
}