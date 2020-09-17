
namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class PolicySettings
    {
        public RetryPolicySettings RetryPolicies { get; set; }
    }

    public class RetryPolicySettings
    {
        public int LockedRetryWaitInMilliSeconds { get; set; }
        public int LockedRetryAttempts { get; set; }

        public int QueryRetryWaitInMilliSeconds { get; set; }
        public int QueryRetryAttempts { get; set; }
    }
}
