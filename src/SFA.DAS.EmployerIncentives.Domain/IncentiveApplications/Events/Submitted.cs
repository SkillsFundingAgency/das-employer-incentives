using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class Submitted : IDomainEvent
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApplicationId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedByEmail { get; set; }        
    }
}
