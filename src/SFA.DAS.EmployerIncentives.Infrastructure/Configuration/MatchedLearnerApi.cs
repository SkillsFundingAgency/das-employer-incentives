using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class MatchedLearnerApi : IManagedIdentityClientConfiguration
    {
        public virtual string ApiBaseUrl { get; set; }
        public string IdentifierUri { get; set; }
        public string Version { get; set; }
    }
}