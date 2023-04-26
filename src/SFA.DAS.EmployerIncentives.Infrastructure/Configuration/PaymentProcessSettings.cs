using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class PaymentProcessSettings
    {
        public List<string> MetricsReportEmailList { get; set; }
        public int ApprovalReminderPeriodSecs { get; set; }
        public int ApprovalReminderRetryAttempts { get; set; }
        public string AuthorisationBaseUrl { get; set; }
    }
}