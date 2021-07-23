using System;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class EmailTemplateIds
    {
        public static string BankDetailsReminder = Guid.NewGuid().ToString();
        public static string BankDetailsRequired = Guid.NewGuid().ToString();
        public static string BankDetailsRepeatReminder = Guid.NewGuid().ToString();
    }
}
