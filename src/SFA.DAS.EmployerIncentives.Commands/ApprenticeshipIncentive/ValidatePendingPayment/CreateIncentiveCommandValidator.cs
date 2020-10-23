using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment
{
    public class ValidatePendingPaymentCommandValidator : IValidator<ValidatePendingPaymentCommand>
    {
        public Task<ValidationResult> Validate(ValidatePendingPaymentCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
