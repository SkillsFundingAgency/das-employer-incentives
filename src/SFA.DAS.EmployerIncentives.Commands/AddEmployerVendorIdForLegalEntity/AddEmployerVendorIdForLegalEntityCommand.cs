using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.AddEmployerVendorIdForLegalEntity
{
    public class AddEmployerVendorIdForLegalEntityCommand : ICommand
    {
        public string HashedLegalEntityId { get; }
        public string EmployerVendorId { get; }

        public AddEmployerVendorIdForLegalEntityCommand(string hashedLegalEntityId, string employerVendorId)
        {
            HashedLegalEntityId = hashedLegalEntityId;
            EmployerVendorId = employerVendorId;
        }
    }
}
