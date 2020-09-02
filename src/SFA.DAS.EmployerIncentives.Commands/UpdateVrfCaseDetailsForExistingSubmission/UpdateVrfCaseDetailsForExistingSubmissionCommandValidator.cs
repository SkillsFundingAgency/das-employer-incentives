using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForExistingSubmission
{
    public class UpdateVrfCaseDetailsForExistingSubmissionCommandValidator : IValidator<UpdateVrfCaseDetailsForExistingSubmissionCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseDetailsForExistingSubmissionCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
