using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class LearningResumed : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public DateTime ResumedDate { get; set; }

        public LearningResumed(
            Guid apprenticeshipIncentiveId,
            DateTime resumedDate)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            ResumedDate = resumedDate;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} " +
                    $"Resumed learning on {ResumedDate}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
