using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.AddLegalEntity;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.AddLegalEntity
{
    public class AddLegalEntityCommandValidator : IValidator<AddLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(AddLegalEntityCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountId == default)
            {
                result.AddError("AccountId", "Is not set");
            }

            if (item.LegalEntityId == default)
            {
                result.AddError("LegalEntityId", "Is not set");
            }

            if (item.AccountLegalEntityId == default)
            {
                result.AddError("AccountLegalEntityId", "Is not set");
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                result.AddError("Name", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
