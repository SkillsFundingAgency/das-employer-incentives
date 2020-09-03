using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForLegalEntity
{
    public class UpdateVrfCaseDetailsForLegalEntityCommandValidator : IValidator<UpdateVrfCaseDetailsForLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseDetailsForLegalEntityCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
