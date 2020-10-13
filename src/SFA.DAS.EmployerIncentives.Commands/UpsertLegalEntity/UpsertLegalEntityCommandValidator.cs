using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity
{
    public class UpsertLegalEntityCommandValidator : IValidator<UpsertLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(UpsertLegalEntityCommand item)
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
