
namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class RetryPolicies
    {
        public int LockedRetryWaitInMilliSeconds { get; set; }
        public int LockedRetryAttempts { get; set; }
    }
}
