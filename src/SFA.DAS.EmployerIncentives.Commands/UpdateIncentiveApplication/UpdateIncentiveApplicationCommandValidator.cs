using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication
{
    public class UpdateIncentiveApplicationCommandValidator : IValidator<UpdateIncentiveApplicationCommand>
    {
        public Task<ValidationResult> Validate(UpdateIncentiveApplicationCommand item)
        {
            var result = new ValidationResult();

            if (item.IncentiveApplicationId == default)
            {
                result.AddError("IncentiveApplicationId", "Is not set");
            }

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            ValidateApprenticeships(result, item.Apprenticeships);

            return Task.FromResult(result);
        }

        private static void ValidateApprenticeships(ValidationResult result, IEnumerable<IncentiveApplicationApprenticeshipDto> apprenticeships)
        {
            if (apprenticeships == null)
            {
                result.AddError("Apprenticeships", "Is not set");
                return;
            }

            if (!apprenticeships.Any())
            {
                result.AddError("Apprenticeships", "Have not been provided");
                return;
            }

            foreach (var apprenticeship in apprenticeships)
            {
                ValidateApprenticeship(result, apprenticeship);
            }
        }

        private static void ValidateApprenticeship(ValidationResult result, IncentiveApplicationApprenticeshipDto apprenticeship)
        {
            if (apprenticeship.ApprenticeshipId == default)
            {
                result.AddError("Apprenticeship.ApprenticeshipId", "Is not set");
            }

            if (apprenticeship.DateOfBirth == default)
            {
                result.AddError("Apprenticeship.DateOfBirth", "Is not set");
            }

            if (apprenticeship.PlannedStartDate == default)
            {
                result.AddError("Apprenticeship.PlannedStartDate", "Is not set");
            }

            if (apprenticeship.Uln == default)
            {
                result.AddError("Apprenticeship.Uln", "Is not set");
            }

            if (string.IsNullOrEmpty(apprenticeship.FirstName))
            {
                result.AddError("Apprenticeship.FirstName", "Is not set");
            }

            if (string.IsNullOrEmpty(apprenticeship.LastName))
            {
                result.AddError("Apprenticeship.LastName", "Is not set");
            }
        }
    }
}
