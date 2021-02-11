using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVrfCaseStatusForAccountCommandValidator : IValidator<UpdateVrfCaseStatusForAccountCommand>
    {
        public Task<ValidationResult> Validate(UpdateVrfCaseStatusForAccountCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
