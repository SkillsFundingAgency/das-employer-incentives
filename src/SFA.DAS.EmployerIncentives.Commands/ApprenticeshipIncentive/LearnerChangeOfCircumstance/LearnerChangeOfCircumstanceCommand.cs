using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class LearnerChangeOfCircumstanceCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public Guid ApprenticeshipIncentiveId { get; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public LearnerChangeOfCircumstanceCommand(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "ApprenticeshipIncentive LearnerChangeOfCircumstanceCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId }),
                    OnProcessed = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId })
                };
            }
        }
    }
}
