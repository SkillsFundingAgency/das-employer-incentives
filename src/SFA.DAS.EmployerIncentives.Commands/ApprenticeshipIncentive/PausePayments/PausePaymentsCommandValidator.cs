using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PausePayments
{
    public class PausePaymentsCommandValidator : IValidator<PausePaymentsCommand>
    {
        public Task<ValidationResult> Validate(PausePaymentsCommand item)
        {
            var result = new ValidationResult();

            if (item.ULN == default)
            {
                result.AddError("ULN", "Is not set");
            }

            if (item.AccountLegalEntityId == default)
            {
                result.AddError("AccountLegalEntityId", "Is not set");
            }

            if (item.ServiceRequestId == default)
            {
                result.AddError("ServiceRequestId", "Is not set");
            }

            if (item.DateServiceRequestTaskCreated == default)
            {
                result.AddError("DateServiceRequestTaskCreated", "Is not set");
            }

            if (string.IsNullOrWhiteSpace(item.DecisionReferenceNumber))
            {
                result.AddError("DecisionReferenceNumber", "Is not set");
            }

            if (item.Action == default)
            {
                result.AddError("Action", "Is not set (it must be Pause or Resume)");
            }

            return Task.FromResult(result);
        }      
    }
}
