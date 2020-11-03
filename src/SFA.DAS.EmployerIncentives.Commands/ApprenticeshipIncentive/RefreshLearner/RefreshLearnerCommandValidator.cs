using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment
{
    public class RefreshLearnerCommandValidator : IValidator<RefreshLearnerCommand>
    {
        public Task<ValidationResult> Validate(RefreshLearnerCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
