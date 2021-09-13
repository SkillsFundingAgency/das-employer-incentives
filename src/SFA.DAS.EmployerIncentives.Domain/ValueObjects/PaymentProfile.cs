using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class PaymentProfile : ValueObject
    {
        public PaymentProfile(IncentiveType incentiveType, int daysAfterApprenticeshipStart, decimal amountPayable)
        {
            DaysAfterApprenticeshipStart = daysAfterApprenticeshipStart;
            AmountPayable = amountPayable;
            IncentiveType = incentiveType;
        }

        public int DaysAfterApprenticeshipStart { get; private set; }
        public decimal AmountPayable { get; private set; }
        public IncentiveType IncentiveType { get; private set; }        

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return DaysAfterApprenticeshipStart;
            yield return AmountPayable;
            yield return IncentiveType;
        }
    }
}