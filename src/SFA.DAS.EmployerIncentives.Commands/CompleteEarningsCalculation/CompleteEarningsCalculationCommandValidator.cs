using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.CompleteEarningsCalculation
{
    public class CompleteEarningsCalculationCommandValidator : IValidator<CompleteEarningsCalculationCommand>
    {
        public Task<ValidationResult> Validate(CompleteEarningsCalculationCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
