using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments
{
    public class ReinstatePendingPaymentCommandValidator : IValidator<ReinstatePendingPaymentCommand>
    {
        public async Task<ValidationResult> Validate(ReinstatePendingPaymentCommand item)
        {
            var result = new ValidationResult();

            if (item.PendingPaymentId == Guid.Empty)
            {
                result.AddError("PendingPaymentId", "Is not set");
            }

            if (item.ReinstatePaymentRequest == null)
            {
                result.AddError("ReinstatePaymentRequest", "Is not set");
            }

            if (String.IsNullOrWhiteSpace(item.ReinstatePaymentRequest.TaskId))
            {
                result.AddError("ReinstatePaymentRequest TaskId", "Is not set");
            }

            if (String.IsNullOrWhiteSpace(item.ReinstatePaymentRequest.DecisionReference))
            {
                result.AddError("ReinstatePaymentRequest DecisionReference", "Is not set");
            }

            if (item.ReinstatePaymentRequest.Created == default)
            {
                result.AddError("ReinstatePaymentRequest Created", "Is not set");
            }

            if (String.IsNullOrWhiteSpace(item.ReinstatePaymentRequest.Process))
            {
                result.AddError("ReinstatePaymentRequest Process", "Is not set");
            }

            return await Task.FromResult(result);
        }
    }
}
