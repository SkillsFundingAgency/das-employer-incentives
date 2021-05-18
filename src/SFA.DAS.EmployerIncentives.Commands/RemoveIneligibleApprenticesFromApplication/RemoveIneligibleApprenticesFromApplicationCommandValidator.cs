using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveIneligibleApprenticesFromApplication
{
    public class RemoveIneligibleApprenticesFromApplicationCommandValidator : IValidator<RemoveIneligibleApprenticesFromApplicationCommand>
    {
        public Task<ValidationResult> Validate(RemoveIneligibleApprenticesFromApplicationCommand item)
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

            return Task.FromResult(result);
        }      
    }
}
