﻿using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders.Configuration
{
    internal class IncentivePaymentProfileListBuilder
    {
        public IncentivePaymentProfileListBuilder()
        {  
        }

        public List<IncentivePaymentProfile> Build()
        {
            return new List<IncentivePaymentProfile>
            {
                 new IncentivePaymentProfile
                {
                    IncentivePhase = Phase.Phase1,
                    PaymentProfiles = new List<PaymentProfile>
                        {
                            new PaymentProfile {IncentiveType = IncentiveType.UnderTwentyFiveIncentive, AmountPayable = 1000, DaysAfterApprenticeshipStart = 89 },
                            new PaymentProfile {IncentiveType = IncentiveType.UnderTwentyFiveIncentive, AmountPayable = 1000, DaysAfterApprenticeshipStart = 364 },
                            new PaymentProfile {IncentiveType = IncentiveType.TwentyFiveOrOverIncentive, AmountPayable = 750, DaysAfterApprenticeshipStart = 89 },
                            new PaymentProfile {IncentiveType = IncentiveType.TwentyFiveOrOverIncentive, AmountPayable = 750, DaysAfterApprenticeshipStart = 364 }
                        },
                },                
                new IncentivePaymentProfile
                {
                    IncentivePhase = Phase.Phase2,
                    PaymentProfiles = new List<PaymentProfile>
                        {
                            new PaymentProfile {IncentiveType = IncentiveType.UnderTwentyFiveIncentive, AmountPayable = 1500, DaysAfterApprenticeshipStart = 89 },
                            new PaymentProfile {IncentiveType = IncentiveType.UnderTwentyFiveIncentive, AmountPayable = 1500, DaysAfterApprenticeshipStart = 364 },
                            new PaymentProfile {IncentiveType = IncentiveType.TwentyFiveOrOverIncentive, AmountPayable = 1500, DaysAfterApprenticeshipStart = 89 },
                            new PaymentProfile {IncentiveType = IncentiveType.TwentyFiveOrOverIncentive, AmountPayable = 1500, DaysAfterApprenticeshipStart = 364 }
                        }
                }
            };            
        }
    }
}