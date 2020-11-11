using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Payment : ValueObject
    {
        public decimal Amount { get; }
        public DateTime PaymentDate { get; }

        public short PaymentNumber { get; }
        public Payment(decimal amount, DateTime paymentDate, short paymentNumber)
        {
            if(amount <= 0) throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            if (paymentNumber < 1 || paymentNumber > 2) throw new ArgumentException("Payment number must be 1 or 2", nameof(paymentNumber));

            Amount = amount;
            PaymentDate = paymentDate;
            PaymentNumber = paymentNumber;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Amount;
            yield return PaymentDate;
            yield return PaymentNumber;
        }
    }
}
