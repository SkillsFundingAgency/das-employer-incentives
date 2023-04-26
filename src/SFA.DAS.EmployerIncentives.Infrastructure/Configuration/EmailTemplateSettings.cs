
namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class EmailTemplateSettings
    {
        public EmailTemplate BankDetailsRequired { get; set; }
        public EmailTemplate BankDetailsReminder { get; set; }
        public EmailTemplate BankDetailsRepeatReminder { get; set; }
        public EmailTemplate ApplicationCancelled { get; set; }
        public EmailTemplate MetricsReport { get; set; }
    }
}
