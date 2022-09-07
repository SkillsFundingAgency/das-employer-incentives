using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ReinstateWithdrawal
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
