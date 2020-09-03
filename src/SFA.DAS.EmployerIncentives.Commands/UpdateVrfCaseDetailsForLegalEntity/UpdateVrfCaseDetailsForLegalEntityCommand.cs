using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForLegalEntity
{
    public class UpdateVrfCaseDetailsForLegalEntityCommand : ICommand
    {
        public long LegalEntityId { get; }
        public string CaseId { get; }
        public string VendorId { get; }
        public string Status { get; }

        public UpdateVrfCaseDetailsForLegalEntityCommand(long legalEntityId, string caseId, string vendorId, string status)
        {
            LegalEntityId = legalEntityId;
            CaseId = caseId;
            VendorId = vendorId;
            Status = status;
        }
    }
}
