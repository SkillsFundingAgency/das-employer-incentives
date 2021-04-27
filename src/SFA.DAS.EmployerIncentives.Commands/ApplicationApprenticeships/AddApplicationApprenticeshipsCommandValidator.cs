using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.ApplicationApprenticeships
{
    public class AddApplicationApprenticeshipsCommandValidator : IValidator<AddApplicationApprenticeshipsCommand>
    {
        public Task<ValidationResult> Validate(AddApplicationApprenticeshipsCommand item)
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

            if (item.Apprenticeships == default)
            {
                result.AddError("Apprenticeships", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
