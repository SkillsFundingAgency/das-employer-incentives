using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.CompleteEarningsCalculation
{
    public class CompleteEarningsCalculationCommandValidator : IValidator<CompleteEarningsCalculationCommand>
    {
        public Task<ValidationResult> Validate(CompleteEarningsCalculationCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveApplicationApprenticeshipId == default)
            {
                result.AddError("IncentiveApplicationApprenticeshipId", "Is not set");
            }

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            if (item.ApprenticeshipId == default)
            {
                result.AddError("ApprenticeshipId", "Is not set");
            }

            if (item.ApprenticeshipIncentiveId == default)
            {
                result.AddError("ApprenticeshipIncentiveId", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
