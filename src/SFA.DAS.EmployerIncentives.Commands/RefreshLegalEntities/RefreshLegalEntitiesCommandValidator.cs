using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities
{
    public class RefreshLegalEntityCommandValidator : IValidator<RefreshLegalEntitiesCommand>
    {
        public Task<ValidationResult> Validate(RefreshLegalEntitiesCommand item)
        {
            var result = new ValidationResult();

            if (item.PageNumber <= 0)
            {
                result.AddError("PageNumber", "Is negative or zero");
            }

            if (item.PageSize <= 0)
            {
                result.AddError("PageSize", "Is negative or zero");
            }

            return Task.FromResult(result);
        }
    }
}
