using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class RemoveApplicationApprenticeshipCommandValidator : IValidator<RemoveApplicationApprenticeshipCommand>
    {
        public Task<ValidationResult> Validate(RemoveApplicationApprenticeshipCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveApplicationId == default)
            {
                result.AddError("IncentiveApplicationId", "Is not set");
            }
            
            if (item.ApprenticeshipId == default)
            {
                result.AddError("ApprenticeshipId", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
