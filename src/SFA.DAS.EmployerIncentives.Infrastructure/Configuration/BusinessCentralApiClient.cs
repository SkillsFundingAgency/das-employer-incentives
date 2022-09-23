using SFA.DAS.EmployerIncentives.Abstractions.API;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class BusinessCentralApiClient : ApimBase
    {
        public const string BusinessCentralApi = "BusinessCentralApi";
        public int PaymentRequestsLimit { get; set; }
        public bool ObfuscateSensitiveData { get; set; }
    }

}