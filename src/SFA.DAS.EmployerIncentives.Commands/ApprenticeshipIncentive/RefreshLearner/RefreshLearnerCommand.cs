using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public class RefreshLearnerCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public Guid ApprenticeshipIncentiveId { get; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public RefreshLearnerCommand(Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "Learner Match record for apprenticeship incentive id { apprenticeshipIncentiveId}";
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>("Creating " + message, new object[] { ApprenticeshipIncentiveId }),
                    OnProcessed = () => new Tuple<string, object[]>("Created " + message, new object[] { ApprenticeshipIncentiveId }),
                    OnError = () => new Tuple<string, object[]>("Created " + message, new object[] { ApprenticeshipIncentiveId })
                };
            }
        }
    }
}
