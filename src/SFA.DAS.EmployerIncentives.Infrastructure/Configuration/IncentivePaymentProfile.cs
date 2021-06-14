using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class IncentivePaymentProfile
    {
        public Phase IncentivePhase { get; set; }
        public List<PaymentProfile> PaymentProfiles { get; set; }
    }
}