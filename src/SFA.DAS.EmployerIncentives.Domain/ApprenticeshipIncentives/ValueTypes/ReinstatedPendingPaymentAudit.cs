using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class ReinstatedPendingPaymentAudit : ValueObject
    {
        public Guid Id { get; }
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PendingPaymentId { get; }
        public ReinstatePaymentRequest ReinstatePaymentRequest { get; }
        public DateTime CreatedDateTime { get; set; }

        public ReinstatedPendingPaymentAudit(
            Guid id,
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            ReinstatePaymentRequest reinstatePaymentRequest,
            DateTime createdDateTime)
        {
            Id = id;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            ReinstatePaymentRequest = reinstatePaymentRequest;
            CreatedDateTime = createdDateTime;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return ApprenticeshipIncentiveId;
            yield return PendingPaymentId;
            yield return ReinstatePaymentRequest;
            yield return CreatedDateTime;
        }
    }
}
