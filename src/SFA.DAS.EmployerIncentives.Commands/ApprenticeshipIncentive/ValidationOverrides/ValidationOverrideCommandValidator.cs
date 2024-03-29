﻿using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
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
                case ValidationStep.IsInLearning:
                case ValidationStep.HasNoDataLocks:
                case ValidationStep.HasDaysInLearning:
                case ValidationStep.EmployedAtStartOfApprenticeship:
                case ValidationStep.EmployedBeforeSchemeStarted:
                case ValidationStep.EmployedAt365Days:
                    break;
                default:
                    result.AddError("ValidationOverrideStep.ValidationStep", "Is not valid");
                    break;
            }

            if ((!step.Remove) && (step.ExpiryDate.Date < System.DateTime.Today))
            {
                result.AddError("ValidationOverrideStep.ExpiryDate", "Is before today's date");
            }
        }
    }    
}
