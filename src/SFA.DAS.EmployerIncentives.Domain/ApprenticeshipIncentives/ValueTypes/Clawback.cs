using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Clawback : ValueObject
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public Account Account { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public Guid PaymentId { get; set; }

        public Clawback(Guid id,
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            Account account,
            decimal amount,
            DateTime createdDate,
            SubnominalCode subnominalCode,
            Guid paymentId
            )
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            Account = account;
            Amount = amount;
            CreatedDate = createdDate;
            SubnominalCode = subnominalCode;
            PaymentId = paymentId;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return PendingPaymentId;
            yield return Account;
            yield return Amount;
            yield return CreatedDate;
            yield return SubnominalCode;
            yield return PaymentId;
        }
    }
}
