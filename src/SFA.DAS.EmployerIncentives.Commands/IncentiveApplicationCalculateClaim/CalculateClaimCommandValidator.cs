using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.Application;

namespace SFA.DAS.EmployerIncentives.Commands.IncentiveApplicationCalculateClaim
{
    public class CalculateClaimCommandValidator : IValidator<CalculateClaimCommand>
    {
        public Task<ValidationResult> Validate(CalculateClaimCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveClaimApplicationId == default)
            {
                result.AddError("IncentiveClaimApplicationId", "Is not set");
            }

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            return Task.FromResult(result);
        }      
    }
}
