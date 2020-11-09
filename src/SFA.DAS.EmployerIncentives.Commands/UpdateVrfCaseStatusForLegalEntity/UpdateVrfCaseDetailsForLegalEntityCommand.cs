using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVendorRegistrationCaseStatusCommand : ICommand
    {
        public string HashedLegalEntityId { get; }
        public string CaseId { get; }
        public string Status { get; }
        public DateTime CaseStatusLastUpdatedDate { get; }

        public UpdateVendorRegistrationCaseStatusCommand(string hashedLegalEntityId, string caseId, string status, DateTime caseStatusLastUpdatedDate)
        {
            HashedLegalEntityId = hashedLegalEntityId;
            CaseId = caseId;
            Status = status;
            CaseStatusLastUpdatedDate = caseStatusLastUpdatedDate;
        }
    }
}
