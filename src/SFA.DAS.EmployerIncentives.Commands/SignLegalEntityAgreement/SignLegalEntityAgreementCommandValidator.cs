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
                result.AddIsNotSetError("AccountId");
            }

            if (item.AccountLegalEntityId == default)
            {
                result.AddIsNotSetError("AccountLegalEntityId");
            }

            if (item.AgreementVersion == default)
            {
                result.AddIsNotSetError("AgreementVersion");
            }

            if (item.LegalEntityId == default)
            {
                result.AddIsNotSetError("LegalEntityId");
            }

            if (item.LegalEntityName == default)
            {
                result.AddIsNotSetError("LegalEntityName");
            }

            return Task.FromResult(result);
        }
    }
}
