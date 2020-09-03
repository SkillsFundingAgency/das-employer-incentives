using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EarningsCalculationRequestedEvent : IDomainEvent
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApplicationId { get; set; }
        public Guid ApprenticeshipId { get; set; }
    }
}
