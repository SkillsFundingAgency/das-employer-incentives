using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForNewApplications
{
    public class UpdateVrfCaseDetailsForNewApplicationsCommandValidator : IValidator<UpdateVrfCaseDetailsForNewApplicationsCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseDetailsForNewApplicationsCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
