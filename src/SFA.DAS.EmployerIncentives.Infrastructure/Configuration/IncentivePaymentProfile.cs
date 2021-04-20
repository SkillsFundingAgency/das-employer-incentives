﻿using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Infrastructure.Configuration
{
    public class IncentivePaymentProfile
    {
        public IncentiveType IncentiveType { get; set; }
        public IncentivePhase IncentivePhase { get; set; }
        public (DateTime Start, DateTime End) EligibleApplicationDates { get; set; }
        public (DateTime Start, DateTime End) EligibleEmploymentDates { get; set; }
        public (DateTime Start, DateTime End) EligibleTrainingDates { get; set; }
        public byte MinRequiredAgreementVersion { get; set; }
        public List<PaymentProfile> PaymentProfiles { get; set; }
    }
}