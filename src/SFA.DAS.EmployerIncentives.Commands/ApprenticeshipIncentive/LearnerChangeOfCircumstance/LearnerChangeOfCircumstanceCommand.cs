using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public DateTime StartDate { get; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public LearnerChangeOfCircumstanceCommand(Guid apprenticeshipIncentiveId, DateTime startDate)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            StartDate = startDate;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive LearnerChangeOfCircumstanceCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
