using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch
{
    public class SetSuccessfulLearnerMatchCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public long Uln { get; }
        public bool Succeeded { get; }
        public string LockId => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}";

        public SetSuccessfulLearnerMatchCommand(Guid apprenticeshipIncentiveId, long uln, bool succeeded)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            Uln = uln;
            Succeeded = succeeded;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive SetSuccessfulLearnerMatchCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, ULN {Uln}, Succeeded {Succeeded} ";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
