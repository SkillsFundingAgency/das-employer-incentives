using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForExistingSubmission
{
    public class UpdateVrfCaseDetailsForExistingSubmissionCommand : ICommand
    {
        public long LegalEntityId { get; }
        public string CaseId { get; }
        public string VendorId { get; }
        public string Status { get; }

        public UpdateVrfCaseDetailsForExistingSubmissionCommand(long legalEntityId, string caseId, string vendorId, string status)
        {
            LegalEntityId = legalEntityId;
            CaseId = caseId;
            VendorId = vendorId;
            Status = status;
        }
    }
}
