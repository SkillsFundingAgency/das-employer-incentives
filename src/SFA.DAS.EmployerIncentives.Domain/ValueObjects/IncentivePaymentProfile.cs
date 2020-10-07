using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivePaymentProfile : ValueObject
    {
        public IncentivePaymentProfile(IncentiveType incentiveType, List<PaymentProfile> paymentProfiles)
        {
            IncentiveType = incentiveType;
            PaymentProfiles = paymentProfiles;
        }

        public IncentiveType IncentiveType { get; }
        public List<PaymentProfile> PaymentProfiles { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return IncentiveType;
            foreach (var paymentProfile in PaymentProfiles)
            {
                yield return paymentProfile;
            }
        }

    }
}