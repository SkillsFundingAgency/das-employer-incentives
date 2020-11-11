using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class EarningsCalculated : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid ApplicationApprenticeshipId { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive EarningsCalculated event with AccountId {AccountId} and ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, ApplicationApprenticeshipId {ApplicationApprenticeshipId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
