using System;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class BreakInLearningDeleted : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }

        public BreakInLearningDeleted(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Break In Learning has been deleted for Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}" ;
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
