using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EarningsCalculationRequestedEvent : IDomainEvent, ILogWriter
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApplicationId { get; set; }
        public Guid ApprenticeshipId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"EarningsCalculationRequestedEvent with AccountId {AccountId}, IncentiveClaimApplicationId {IncentiveClaimApplicationId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
