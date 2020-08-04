using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{
    public class SignLegalEntityAgreementCommandValidator : IValidator<SignLegalEntityAgreementCommand>
    {
        public Task<ValidationResult> Validate(SignLegalEntityAgreementCommand item)
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

            if (item.AgreementVersion == default)
            {
                result.AddError("AgreementVersion", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
