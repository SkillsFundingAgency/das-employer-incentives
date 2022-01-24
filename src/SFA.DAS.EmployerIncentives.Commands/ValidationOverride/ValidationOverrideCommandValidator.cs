using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ValidationOverrides;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ValidationOverride
{
    public class ValidationOverrideCommandValidator : IValidator<ValidationOverrideCommand>
    {
        public Task<ValidationResult> Validate(ValidationOverrideCommand item)
        {
            var result = new ValidationResult();

            return Task.FromResult(result);
        }
    }
}
