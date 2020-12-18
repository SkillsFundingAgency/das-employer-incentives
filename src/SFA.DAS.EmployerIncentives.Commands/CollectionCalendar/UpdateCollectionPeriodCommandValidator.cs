using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CollectionCalendar
{
    public class UpdateCollectionPeriodCommandValidator : IValidator<UpdateCollectionPeriodCommand>
    {
        public Task<ValidationResult> Validate(UpdateCollectionPeriodCommand item)
        {
            var result = new ValidationResult();
            if (item.PeriodNumber < 1 || item.PeriodNumber > 12)
            {
                result.AddError("PeriodNumber", "Should be between 1 and 12");
            }
            if (item.AcademicYear == default)
            {
                result.AddError("AcademicYear", "Should be set");
            }
            else if (item.AcademicYear.Length < 4)
            {
                result.AddError("AcademicYear", "Should be 4 digits");
            }
            else
            {
                int academicYear;
                var isNumeric = int.TryParse(item.AcademicYear, out academicYear);
                if (!isNumeric)
                {
                    result.AddError("AcademicYear", "Should be numeric");
                }
            }

            return Task.FromResult(result);
        }
    }
}
