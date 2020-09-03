using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForIncompleteCases
{
    public class UpdateVrfCaseDetailsForIncompleteCasesCommandValidator : IValidator<UpdateVrfCaseDetailsForIncompleteCasesCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseDetailsForIncompleteCasesCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
