using System;

namespace SFA.DAS.EmployerIncentives.Messages.Events
{
    public class EmployerIncentiveClaimSubmittedEvent
    {
        public Guid IncentiveClaimApprenticeshipId { get; set; }
    }
}
