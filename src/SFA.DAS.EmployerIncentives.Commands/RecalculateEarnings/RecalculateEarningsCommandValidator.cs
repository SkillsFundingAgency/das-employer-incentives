using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.RecalculateEarnings
{
    public class RecalculateEarningsCommandValidator : IValidator<RecalculateEarningsCommand>
    {
        public Task<ValidationResult> Validate(RecalculateEarningsCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveLearnerIdentifiers == null || !item.IncentiveLearnerIdentifiers.Any())
            {
                result.AddError("IncentiveLearnerIdentifiers", "Is not set");
                return Task.FromResult(result);
            }

            foreach (var apprenticeshipIdentifier in item.IncentiveLearnerIdentifiers)
            {
                ValidateApprenticeshipIdentifier(result, apprenticeshipIdentifier);
            }
            
            return Task.FromResult(result);
        }

        private void ValidateApprenticeshipIdentifier(ValidationResult result, IncentiveLearnerIdentifierDto apprenticeshipIdentifier)
        {
            if (apprenticeshipIdentifier.AccountLegalEntityId == default)
            {
                result.AddError("IncentiveLearnerIdentifier AccountLegalEntityId", "Is not set");
            }

            if (apprenticeshipIdentifier.ULN == default)
            {
                result.AddError("IncentiveLearnerIdentifier ULN", "Is not set");
            }
        }
    }
}
