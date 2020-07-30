using SFA.DAS.EAS.Account.Api.Client;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class ApplicationSettings
    {
        public string DbConnectionString { get; set; }
        public string DistributedLockStorage { get; set; }
        public string LockedRetryPolicyInMilliSeconds { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public string Hashstring { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string NServiceBusLicense { get; set; }      
        public virtual int MinimumAgreementVersion { get; set; }
    }
}
