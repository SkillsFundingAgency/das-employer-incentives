using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommandValidator : IValidator<RemoveLegalEntityCommand>
    {
        public Task<ValidationResult> Validate(RemoveLegalEntityCommand item)
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

            return Task.FromResult(result);
        }
    }
}
