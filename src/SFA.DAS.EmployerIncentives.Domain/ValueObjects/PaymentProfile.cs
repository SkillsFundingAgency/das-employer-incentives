using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class PaymentProfile : ValueObject
    {
        public PaymentProfile(int daysAfterApprenticeshipStart, decimal amountPayable)
        {
            DaysAfterApprenticeshipStart = daysAfterApprenticeshipStart;
            AmountPayable = amountPayable;
        }

        public int DaysAfterApprenticeshipStart { get; set; }
        public decimal AmountPayable { get; set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return DaysAfterApprenticeshipStart;
            yield return AmountPayable;
        }
    }
}