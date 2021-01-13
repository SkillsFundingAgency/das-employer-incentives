using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.AccountVrfCaseStatus
{
    public class AccountVrfCaseStatusRemindersCommandValidator : IValidator<AccountVrfCaseStatusRemindersCommand>
    {
        public Task<ValidationResult> Validate(AccountVrfCaseStatusRemindersCommand item)
        {
            var result = new ValidationResult();

            if (item.ApplicationCutOffDate == default)
            {
                result.AddError("Application cut off date", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
