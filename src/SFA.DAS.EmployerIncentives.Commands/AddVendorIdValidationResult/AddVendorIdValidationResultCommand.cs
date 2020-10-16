using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.AddVendorIdValidationResult
{
    public class AddVendorIdValidationResultCommand : ICommand
    {
        public long AccountLegalEntityId { get; }

        public AddVendorIdValidationResultCommand(long accountLegalEntityId)
        {
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
