using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class VendorCase
    {
        public string HashedLegalEntityId { get; set; }
        public string Status { get; set; }
        public string CaseId { get; set; }
        public DateTime CaseStatusLastUpdatedDate { get; set; }
    }
}