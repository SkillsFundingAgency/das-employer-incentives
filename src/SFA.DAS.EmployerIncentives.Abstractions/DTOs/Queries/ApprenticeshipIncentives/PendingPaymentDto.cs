using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives
{
    public class PendingPaymentDto
    {
        public Guid Id { get; set; }
        public short? PaymentPeriod { get; set; }
        public short? PaymentYear { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime? PaymentMadeDate { get; set; }
    }
}
