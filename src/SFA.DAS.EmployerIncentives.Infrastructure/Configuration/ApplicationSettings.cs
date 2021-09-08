using SFA.DAS.Http.Configuration;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class ApplicationSettings : IManagedIdentityClientConfiguration
    {
        public string DbConnectionString { get; set; }
        public string DistributedLockStorage { get; set; }
        public string LockedRetryPolicyInMilliSeconds { get; set; }
        public string AllowedHashstringCharacters { get; set; }
        public string Hashstring { get; set; }
        public string NServiceBusConnectionString { get; set; }
        public string NServiceBusLicense { get; set; }
        public string UseLearningEndpointStorageDirectory { get; set; }        
        public virtual int MinimumAgreementVersion { get; set; }
        public string ApiBaseUrl { get; set; }
        public string Identifier { get; set; }
        public string EmployerIncentivesWebBaseUrl { get; set; }
    }
}