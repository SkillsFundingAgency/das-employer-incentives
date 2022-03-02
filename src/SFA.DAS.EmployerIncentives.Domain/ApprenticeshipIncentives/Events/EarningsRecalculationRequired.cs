using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class EarningsRecalculationRequired : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive EarningsRecalculationRequired event with AccountId {AccountId} and ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
