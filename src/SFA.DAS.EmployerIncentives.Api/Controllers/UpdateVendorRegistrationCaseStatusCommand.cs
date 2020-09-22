using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    public class UpdateVendorRegistrationCaseStatusCommand : ICommand
    {
        public string HashedLegalEntityId { get; }
        public string CaseId { get; }
        public string VendorId { get; }
        public string Status { get; }
        public DateTime CaseStatusLastUpdatedDate { get; }

        public UpdateVendorRegistrationCaseStatusCommand(string hashedLegalEntityId, string caseId, string vendorId, string status, in DateTime caseStatusLastUpdatedDate)
        {
            HashedLegalEntityId = hashedLegalEntityId;
            CaseId = caseId;
            VendorId = vendorId;
            Status = status;
            CaseStatusLastUpdatedDate = caseStatusLastUpdatedDate;
        }
    }
}