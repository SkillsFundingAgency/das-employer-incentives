using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
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
                result.AddIsNotSetError(nameof(item.AccountLegalEntityId));
            }

            if (item.ULN == default)
            {
                result.AddIsNotSetError(nameof(item.ULN));
            }

            return Task.FromResult(result);
        }
    }
}
