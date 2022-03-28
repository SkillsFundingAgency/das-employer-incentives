using System;

namespace SFA.DAS.EmployerIncentives.DataTransferObjects
{
    public class PendingPaymentActivityDto
    {
        public Guid PendingPaymentId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
    }
}