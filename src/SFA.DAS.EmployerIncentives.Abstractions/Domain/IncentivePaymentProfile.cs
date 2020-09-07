using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain
{
    public class IncentivePaymentProfile
    {
        public IncentiveType IncentiveType { get; set; }
        public List<PaymentProfile> PaymentProfiles { get; set; }
    }
}