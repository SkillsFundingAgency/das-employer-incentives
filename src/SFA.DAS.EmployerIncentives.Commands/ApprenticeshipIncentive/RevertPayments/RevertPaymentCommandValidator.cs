using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RevertPayments
{
    public class RevertPaymentCommandValidator : IValidator<RevertPaymentCommand>
    {
        public Task<ValidationResult> Validate(RevertPaymentCommand item)
        {
            var result = new ValidationResult();

            if (item.PaymentId == Guid.Empty)
            {
                result.AddError("PaymentId", "Is not set");
            }

            if (String.IsNullOrWhiteSpace(item.ServiceRequestId))
            {
                result.AddError("ServiceRequestId", "Is not set");
            }

            if (String.IsNullOrWhiteSpace(item.DecisionReferenceNumber))
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
