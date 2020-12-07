using System;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class BankDetailsReminderEmailRequest
    {
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public Guid ApplicationId { get; set; }
        public string EmailAddress { get; set; }
    }
}
