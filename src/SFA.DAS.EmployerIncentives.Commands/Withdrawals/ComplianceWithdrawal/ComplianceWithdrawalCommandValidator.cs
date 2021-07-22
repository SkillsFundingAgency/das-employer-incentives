using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ComplianceWithdrawalCommandValidator : IValidator<ComplianceWithdrawalCommand>
    {
        public Task<ValidationResult> Validate(ComplianceWithdrawalCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountLegalEntityId == default)
            {
                result.AddError("AccountLegalEntityId", "Is not set");
            }

            if (item.ULN == default)
            {
                result.AddError("ULN", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
