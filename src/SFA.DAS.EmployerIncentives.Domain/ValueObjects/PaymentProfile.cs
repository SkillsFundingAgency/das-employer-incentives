using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class PaymentProfile : ValueObject
    {
        public PaymentProfile(int daysAfterApprenticeshipStart, decimal amountPayable, IncentiveType incentiveType)
        {
            DaysAfterApprenticeshipStart = daysAfterApprenticeshipStart;
            AmountPayable = amountPayable;
            IncentiveType = incentiveType;
        }

        public int DaysAfterApprenticeshipStart { get; }
        public decimal AmountPayable { get; }
        public IncentiveType IncentiveType { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return DaysAfterApprenticeshipStart;
            yield return AmountPayable;
            yield return IncentiveType;
        }
    }
}