using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Abstractions.API
{
    public class ApimBase : ManagedIdentityApiBase, IApimClientConfiguration
    {
        public string SubscriptionKey { get; set; }

        public string ApiVersion
        {
            get => base.Version;
            set => base.Version = value;
        }
    }
}
