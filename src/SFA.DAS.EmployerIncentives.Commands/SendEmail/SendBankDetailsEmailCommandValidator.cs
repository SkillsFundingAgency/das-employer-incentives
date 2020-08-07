using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsEmailCommandValidator : IValidator<SendBankDetailsEmailCommand>
    {
        public Task<ValidationResult> Validate(SendBankDetailsEmailCommand item)
        {
            var result = new ValidationResult();

            if (item.AccountId == default)
            {
                result.AddError("Account Id", "Is not set");
            }

            if(item.AccountLegalEntityId == default)
            {
                result.AddError("Account Legal Entity Id", "Is not set");
            }

            if (item.EmailAddress == default)
            {
                result.AddError("Email Address", "Is not set");
            }

            if (item.AddBankDetailsUrl == default)
            {
                result.AddError("Add Bank Details", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
