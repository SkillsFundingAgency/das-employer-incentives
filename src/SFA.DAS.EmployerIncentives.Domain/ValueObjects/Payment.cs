using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Payment : ValueObject
    {
        public decimal Amount { get; }
        public DateTime PaymentDate { get; }

        public EarningType EarningType { get; }
        public Payment(decimal amount, DateTime paymentDate, EarningType earningType)
        {
            if(amount <= 0) throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            Amount = amount;
            PaymentDate = paymentDate;
            EarningType = earningType;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Amount;
            yield return PaymentDate;
            yield return EarningType;
        }
    }
}
