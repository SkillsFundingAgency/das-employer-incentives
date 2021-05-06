using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders
{
    internal class IncentivePaymentProfileListBuilder
    {
        private readonly List<IncentivePaymentProfile> _incentivePaymentProfiles;

        public IncentivePaymentProfileListBuilder()
        {
            _incentivePaymentProfiles = new List<IncentivePaymentProfile>
            {
                 new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase1_0),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 89, 1000),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 364, 1000),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 89, 750),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 364, 750)
                            }),

                  new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase1_1),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 89, 1000),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 364, 1000),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 89, 750),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 364, 750)
                            }),

                   new IncentivePaymentProfile(
                     new IncentivePhase(Phase.Phase2_0),
                        new List<PaymentProfile>
                            {
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 89, 1500),
                                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, 364, 1500),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 89, 1500),
                                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, 364, 1500)
                            })
            };
        }

        public IncentivePaymentProfileListBuilder WithIncentivePaymentProfiles(List<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            _incentivePaymentProfiles.Clear();
            _incentivePaymentProfiles.AddRange(incentivePaymentProfiles);
            return this;
        }

        public List<IncentivePaymentProfile> Build()
        {
            return _incentivePaymentProfiles;
        }
    }
}
