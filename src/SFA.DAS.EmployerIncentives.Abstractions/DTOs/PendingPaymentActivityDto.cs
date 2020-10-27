using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs
{
    public class PendingPaymentActivityDto
    {
        public Guid PendingPaymentId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
    }
}