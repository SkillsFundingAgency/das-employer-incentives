using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl
{
    public class EmployerWithdrawlCommandValidator : IValidator<EmployerWithdrawlCommand>
    {
        public Task<ValidationResult> Validate(EmployerWithdrawlCommand item)
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
