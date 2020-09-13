using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandValidator : IValidator<CalculateEarningsCommand>
    {
        public Task<ValidationResult> Validate(CalculateEarningsCommand item)
        {
            var result = new ValidationResult();

            if (item.ApprenticeshipIncentiveId == default)
            {
                result.AddError("ApprenticeshipIncentiveId", "Is not set");
            }

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            if (item.ApprenticeshipId == default)
            {
                result.AddError("ApprenticeshipId", "Is not set");
            }

            return Task.FromResult(result);
        }      
    }
}
