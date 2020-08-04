using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class SubmitIncentiveApplicationCommandValidator : IValidator<SubmitIncentiveApplicationCommand>
    {
        public Task<ValidationResult> Validate(SubmitIncentiveApplicationCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveApplicationId == default)
            {
                result.AddError("IncentiveApplicationId", "Is not set");
            }

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            if (item.DateSubmitted == default)
            {
                result.AddError("DateSubmitted", "Is not set");
            }

            if (item.SubmittedBy == default)
            {
                result.AddError("SubmittedBy", "Is not set");
            }

            return Task.FromResult(result);
        }      
    }
}
