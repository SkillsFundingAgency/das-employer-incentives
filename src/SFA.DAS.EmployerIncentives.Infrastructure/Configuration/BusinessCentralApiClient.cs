using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class BusinessCentralApiClient : IApimClientConfiguration
    {
        public const string BusinessCentralApi = "BusinessCentralApi";
        public string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }
        public string ApiVersion { get; set; }
        public int PaymentRequestsLimit { get; set; }
    }
    
}