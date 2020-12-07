using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsRepeatReminderEmailCommandValidator : IValidator<SendBankDetailsRepeatReminderEmailCommand>
    {
        public Task<ValidationResult> Validate(SendBankDetailsRepeatReminderEmailCommand item)
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

            if (item.ApplicationId == default)
            {
                result.AddError("Application Id", "Is not set");
            }

            return Task.FromResult(result);
        }
    }
}
