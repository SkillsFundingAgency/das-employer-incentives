using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public string VendorId { get; set; }
    }
}
