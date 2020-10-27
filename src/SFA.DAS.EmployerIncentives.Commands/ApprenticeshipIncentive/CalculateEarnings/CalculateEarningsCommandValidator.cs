using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateEarnings
{
    public class CalculateEarningsCommandValidator : IValidator<CalculateEarningsCommand>
    {
        public Task<ValidationResult> Validate(CalculateEarningsCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
