using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess
{
    public class CreatePaymentInput
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; }
    }
}
