using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck
{
    public class EarningsResilienceCheckCommandValidator : IValidator<EarningsResilienceCheckCommand>
    {
        public Task<ValidationResult> Validate(EarningsResilienceCheckCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
