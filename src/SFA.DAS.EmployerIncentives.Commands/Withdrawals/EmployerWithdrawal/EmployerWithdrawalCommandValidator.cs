using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal
{
    public class EmployerWithdrawalCommandValidator : IValidator<EmployerWithdrawalCommand>
    {
        public Task<ValidationResult> Validate(EmployerWithdrawalCommand item)
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
