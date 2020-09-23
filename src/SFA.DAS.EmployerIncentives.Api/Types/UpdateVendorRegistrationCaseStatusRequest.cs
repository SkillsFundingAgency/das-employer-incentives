using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateVendorRegistrationCaseStatusRequest
    {
        public string HashedLegalEntityId { get; set; }
        public string Status { get; set; }
        public string VendorId { get; set; }
        public string CaseId { get; set; }
        public DateTime CaseStatusLastUpdatedDate { get; set; }
    }
}