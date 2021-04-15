using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreateIncentive
{
    public class CreateIncentiveCommandValidator : IValidator<CreateIncentiveCommand>
    {
        public Task<ValidationResult> Validate(CreateIncentiveCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }
            
            if (item.AccountLegalEntityId == default)
            {
                result.AddError("AccountLegalEntityId", "Is not set");
            }

            if (item.ApprenticeshipId == default)
            {
                result.AddError("ApprenticeshipId", "Is not set");
            }

            if (item.DateOfBirth == default)
            {
                result.AddError("DateOfBirth", "Is not set");
            }

            if (item.FirstName == default)
            {
                result.AddError("FirstName", "Is not set");
            }

            if (item.IncentiveApplicationApprenticeshipId == default)
            {
                result.AddError("IncentiveApplicationApprenticeshipId", "Is not set");
            }

            if (item.LastName == default)
            {
                result.AddError("LastName", "Is not set");
            }

            if (item.PlannedStartDate == default)
            {
                result.AddError("PlannedStartDate", "Is not set");
            }

            if (!item.UKPRN.HasValue || item.UKPRN.Value == default)
            {
                result.AddError("UKPRN", "Is not set");
            }

            if (item.Uln == default)
            {
                result.AddError("Uln", "Is not set");
            }

            if (item.SubmittedDate == default)
            {
                result.AddError("SubmittedDate", "Is not set");
            }

            if (item.SubmittedByEmail == default)
            {
                result.AddError("SubmittedByEmail", "Is not set");
            }

            if (item.CourseName == default)
            {
                result.AddError("CourseName", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
