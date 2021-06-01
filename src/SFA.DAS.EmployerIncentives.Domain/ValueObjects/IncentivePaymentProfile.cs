using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivePaymentProfile : ValueObject
    {
        public IncentivePaymentProfile(IncentivePhase incentivePhase, List<PaymentProfile> paymentProfiles)
        {
            IncentivePhase = incentivePhase;
            PaymentProfiles = paymentProfiles;
        }

        public IncentivePhase IncentivePhase { get; }
        public List<PaymentProfile> PaymentProfiles { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return IncentivePhase.Identifier;
            foreach (var paymentProfile in PaymentProfiles)
            {
                yield return paymentProfile;
            }
        }

    }
}