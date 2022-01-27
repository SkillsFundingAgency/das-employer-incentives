using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidationOverrides
{
    public class ValidationOverrideCommandValidator : IValidator<ValidationOverrideCommand>
    {
        public Task<ValidationResult> Validate(ValidationOverrideCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountLegalEntityId == default)
            {
                result.AddIsNotSetError(nameof(item.AccountLegalEntityId));
            }

            if (item.ULN == default)
            {
                result.AddIsNotSetError(nameof(item.ULN));
            }

            if (item.ServiceRequestTaskId == default)
            {
                result.AddError("ServiceRequestTaskId", "Is not set");
            }

            if (string.IsNullOrWhiteSpace(item.DecisionReference))
            {
                result.AddError("DecisionReference", "Is not set");
            }

            if (item.ValidationSteps == null)
            {
                result.AddError("ValidationSteps", "Is not set");
            }
            else
            {
                item.ValidationSteps.ToList().ForEach(s => Validate(s, result));
            }

            return Task.FromResult(result);
        }

        private void Validate(ValidationOverrideStep step, ValidationResult result)
        {
            switch(step.ValidationType)
            {
                case ValidationStep.HasBankDetails:
                case ValidationStep.IsInLearning:
                case ValidationStep.HasLearningRecord:
                case ValidationStep.HasNoDataLocks:
                case ValidationStep.HasIlrSubmission:
                case ValidationStep.HasDaysInLearning:
                case ValidationStep.PaymentsNotPaused:
                case ValidationStep.HasSignedMinVersion:
                case ValidationStep.LearnerMatchSuccessful:
                case ValidationStep.EmployedAtStartOfApprenticeship:
                case ValidationStep.EmployedBeforeSchemeStarted:
                    break;
                default:
                    result.AddError("ValidationOverrideStep.ValidationStep", "Is not valild");
                    break;
            }

            if (step.ExpiryDate.Date < System.DateTime.Today)
            {
                result.AddError("ValidationOverrideStep.ExpiryDate", "Is before today's date");
            }
        }
    }    
}
