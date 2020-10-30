using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class EarningsResilienceCheckDto
    {
        public long AccountId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
