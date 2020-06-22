
namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class FunctionSettings
    {
        public string DbConnectionString { get; set; }
        public string DistributedLockStorage { get; set; }
        public string LockedRetryPolicyInMilliSeconds { get; set; }
    }

    public class RetryPolicies
    {
        public int LockedRetryWaitInMilliSeconds { get; set; }
        public int LockedRetryAttempts { get; set; }
    }
}
