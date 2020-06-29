using SFA.DAS.NServiceBus;
using System;

namespace SFA.DAS.EmployerIncentives.Messages
{
    public class EmployerIncentiveClaimSubmittedEvent : Event
    {
        public Guid IncentiveClaimApprenticeshipId { get; set; }
    }
}
