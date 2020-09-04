using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EarningsCalculationRequested : IDomainEvent, ILogWriter
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApprenticeshipId { get; set; }
        public long ApprenticeshipId { get; set; }
        public IncentiveType IncentiveType { get; set; }
        public DateTime ApprenticeshipStartDate { get; set; }

        public Log Log
        {
            get
            {
                var message = $"EarningsCalculationRequested event with AccountId {AccountId}, IncentiveClaimApprenticeshipId {IncentiveClaimApprenticeshipId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
