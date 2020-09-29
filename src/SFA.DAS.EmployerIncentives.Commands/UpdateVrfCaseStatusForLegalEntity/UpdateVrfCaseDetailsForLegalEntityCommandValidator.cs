using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVendorRegistrationCaseStatusCommandValidator : IValidator<UpdateVendorRegistrationCaseStatusCommand>
    {
        public Task<ValidationResult> Validate(UpdateVendorRegistrationCaseStatusCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
