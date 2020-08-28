using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForNewSubmissions
{
    public class UpdateVrfCaseDetailsForNewSubmissionsCommandValidator : IValidator<UpdateVrfCaseDetailsForNewSubmissionsCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseDetailsForNewSubmissionsCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
