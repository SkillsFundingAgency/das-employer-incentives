using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.GetVrfCaseDetailsForNewSubmissions
{
    public class GetVrfCaseDetailsForNewSubmissionsCommandValidator : IValidator<GetVrfCaseDetailsForNewSubmissionsCommand>
    {
        public Task<ValidationResult> Validate(GetVrfCaseDetailsForNewSubmissionsCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
