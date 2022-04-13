using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives
{
    public class PendingPayment
    {
        public Guid Id { get; set; }
        public byte? PeriodNumber { get; set; }
        public short? PaymentYear { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime? PaymentMadeDate { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }

    }
}
