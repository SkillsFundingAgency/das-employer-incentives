using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class ApplicationSettings : IManagedIdentityClientConfiguration
    {
        public string DbConnectionString { get; set; }
        public string DistributedLockStorage { get; set; }
        public string LockedRetryPolicyInMilliSeconds { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string NServiceBusLicense { get; set; }
        public string UseLearningEndpointStorageDirectory { get; set; }        
        public virtual int MinimumAgreementVersion { get; set; }
        public string ApiBaseUrl { get; set; }
        public string IdentifierUri { get; set; }
        public string EmployerIncentivesWebBaseUrl { get; set; }
        public string LogLevel { get; set; }
        public bool EmploymentCheckEnabled { get; set; }
        public string ReportsConnectionString { get; set; }
        public string ReportsContainerName { get; set; }
        public int? LearnerServiceCacheIntervalInMinutes { get; set; }
    }
}