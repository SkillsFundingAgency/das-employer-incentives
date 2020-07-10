using System;

namespace SFA.DAS.EmployerIncentives.Messages.Events
{
    public class EmployerIncentiveClaimSubmittedEvent
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApprenticeshipId { get; set; }
    }
}
