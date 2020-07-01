
namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string DbConnectionString { get; set; }
        public string DistributedLockStorage { get; set; }
        public string LockedRetryPolicyInMilliSeconds { get; set; }
    }
}
