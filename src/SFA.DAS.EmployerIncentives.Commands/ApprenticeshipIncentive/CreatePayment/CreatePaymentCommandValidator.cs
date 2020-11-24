using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment
{
    public class CreatePaymentCommandValidator : IValidator<CreatePaymentCommand>
    {
        public Task<ValidationResult> Validate(CreatePaymentCommand item)
        {
            return Task.FromResult(new ValidationResult());
        }
    }
}
