using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events
{
    public class LearningStopped : IDomainEvent, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; set; }
        public DateTime StoppedDate { get; set; }

        public LearningStopped(
            Guid apprenticeshipIncentiveId,
            DateTime stoppedDate)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            StoppedDate = stoppedDate;
        }

        public Log Log
        {
            get
            {
                var message =
                    $"Apprenticeship Incentive with ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} " +
                    $"Stopped learning on {StoppedDate}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
