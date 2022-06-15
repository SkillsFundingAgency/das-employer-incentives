using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments
{
    public class RevertPaymentCommandValidator : IValidator<RevertPaymentCommand>
    {
        public Task<ValidationResult> Validate(RevertPaymentCommand item)
        {
            var result = new ValidationResult();

            if (item.PaymentId == default)
            {
                result.AddError("PaymentId", "Is not set");
            }

            if (item.ServiceRequestId == default)
            {
                result.AddError("ServiceRequestId", "Is not set");
            }

            if (item.DecisionReferenceNumber == default)
            {
                result.AddError("DecisionReferenceNumber", "Is not set");
            }
            
            if (item.DateServiceRequestTaskCreated == default)
            {
                result.AddError("DateServiceRequestTaskCreated", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
