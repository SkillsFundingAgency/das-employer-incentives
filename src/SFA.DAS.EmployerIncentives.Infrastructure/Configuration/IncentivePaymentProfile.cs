using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class IncentivePaymentProfile
    {
        public (DateTime Start, DateTime End) EligibleApplicationDates { get; set; }
        public (DateTime Start, DateTime End) EligibleTrainingDates { get; set; }
        public byte MinRequiredAgreementVersion { get; set; }
        public List<PaymentProfile> PaymentProfiles { get; set; }
    }
}