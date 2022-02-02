using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ReinstateWithdrawalCommandValidator : IValidator<ReinstateWithdrawalCommand>
    {
        public Task<ValidationResult> Validate(ReinstateWithdrawalCommand item)
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
