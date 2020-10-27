using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateIncentiveCommandValidator : IValidator<CreateIncentiveCommand>
    {
        public Task<ValidationResult> Validate(CreateIncentiveCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
