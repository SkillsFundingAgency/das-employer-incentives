using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar
{
    public class ActivateCollectionPeriodCommandValidator : IValidator<ActivateCollectionPeriodCommand>
    {
        public Task<ValidationResult> Validate(ActivateCollectionPeriodCommand item)
        {
            var result = new ValidationResult();
            if (item.CollectionPeriodNumber < 1 || item.CollectionPeriodNumber > 12)
            {
                result.AddError("CollectionPeriodNumber", "Should be between 1 and 12");
            }
            var startYear = ValueObjects.NewApprenticeIncentive.EligibilityStartDate.Year;
            if (item.CollectionPeriodYear < startYear)
            {
                result.AddError("CollectionPeriodYear", $"Should be at least {startYear}");
            }

            return Task.FromResult(result);
        }
    }
}
