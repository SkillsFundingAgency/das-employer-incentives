using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class RevertedPaymentAudit : ValueObject
    {
        public Guid Id { get; } 
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PaymentId { get; }
        public Guid PendingPaymentId { get; }
        public byte PaymentPeriod { get; }
        public short PaymentYear { get; }
        public decimal Amount { get; }
        public DateTime CalculatedDate { get; }
        public DateTime PaidDate { get;}
        public string VrfVendorId { get;  }
        public ServiceRequest ServiceRequest { get; }
        public DateTime CreatedDateTime { get; set; }

        public RevertedPaymentAudit(
            Guid id, 
            Guid apprenticeshipIncentiveId,
            Guid paymentId,
            Guid pendingPaymentId,
            byte paymentPeriod,
            short paymentYear,
            decimal amount,
            DateTime calculatedDate,
            DateTime paidDate,
            string vrfVendorId,
            ServiceRequest serviceRequest,
            DateTime createdDateTime)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PaymentId = paymentId;
            PendingPaymentId = pendingPaymentId;
            PaymentPeriod = paymentPeriod;
            PaymentYear = paymentYear;
            Amount = amount;
            CalculatedDate = calculatedDate;
            PaidDate = paidDate;
            VrfVendorId = vrfVendorId;
            ServiceRequest = serviceRequest;
            CreatedDateTime = createdDateTime;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return PaymentId;
            yield return PendingPaymentId;
            yield return PaymentPeriod;
            yield return PaymentYear;
            yield return Amount;
            yield return CalculatedDate;
            yield return PaidDate;
            yield return VrfVendorId;
            yield return ServiceRequest;
            yield return CreatedDateTime;
        }
    }
}
