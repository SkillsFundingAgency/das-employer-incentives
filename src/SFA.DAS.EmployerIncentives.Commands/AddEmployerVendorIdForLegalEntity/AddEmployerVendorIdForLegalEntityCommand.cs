using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
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
