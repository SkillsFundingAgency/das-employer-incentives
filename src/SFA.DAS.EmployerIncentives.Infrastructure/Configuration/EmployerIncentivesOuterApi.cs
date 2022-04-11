using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class EmployerIncentivesOuterApi : IApimClientConfiguration
    {
        public virtual string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }
        public string ApiVersion { get; set; }
    }
}