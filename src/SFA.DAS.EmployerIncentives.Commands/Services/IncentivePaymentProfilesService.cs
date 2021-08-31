using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class IncentivePaymentProfilesService : IIncentivePaymentProfilesService
    {
        public Task<IEnumerable<IncentivePaymentProfile>> Get()
        {
            var incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile(new IncentivePhase(Phase.Phase1),
                    new List<PaymentProfile>
                    {
                        new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1000),
                        new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1000),
                        new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 750),
                        new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 750)
                    }),
                new IncentivePaymentProfile(new IncentivePhase(Phase.Phase2),
                    new List<PaymentProfile>
                    {
                        new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                        new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500),
                        new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                        new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500)
                    }),
            };

            return Task.FromResult(incentivePaymentProfiles.AsEnumerable());
        }
    }
}
