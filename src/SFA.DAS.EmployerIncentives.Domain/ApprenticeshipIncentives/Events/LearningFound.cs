using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class LearningFound : IDomainEvent, ILogWriter
    {
        public LearningFound(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        public Guid ApprenticeshipIncentiveId { get; set; }
        public Log Log
        {
            get
            {
                var message = $"Learning found for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
