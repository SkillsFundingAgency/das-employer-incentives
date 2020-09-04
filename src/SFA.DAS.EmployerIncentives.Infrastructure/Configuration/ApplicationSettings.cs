using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

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
        public List<IncentivePaymentProfile> IncentivePaymentProfiles { get; set; }
    }

    public class IncentivePaymentProfile
    {
        public IncentiveType IncentiveType { get; set; }
        public List<PaymentProfile> PaymentProfiles { get; set; }
    }

    public class PaymentProfile
    {
        public int DaysAfterApprenticeshipStart { get; set; }
        public decimal AmountPayable { get; set; }
    }
}