using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.SendEmail
{
    public class SendBankDetailsRepeatReminderEmailCommand : ICommand
    {
        public long AccountId { get; private set; }
        public long AccountLegalEntityId { get; private set; }
        public string EmailAddress { get; private set; }
        public Guid ApplicationId { get; private set; }

        public SendBankDetailsRepeatReminderEmailCommand(long accountId, long accountLegalEntityId, Guid applicationId, string emailAddress)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            ApplicationId = applicationId;
            EmailAddress = emailAddress;
        }
    }
}
