using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatchExecution
{
    public class SetSuccessfulLearnerMatchExecutionCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public long Uln { get; }
        public bool Succeeded { get; }
        public string LockId => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}";

        public SetSuccessfulLearnerMatchExecutionCommand(Guid apprenticeshipIncentiveId, long uln, bool succeeded)
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
                var message = $"ApprenticeshipIncentive SetSuccessfulLearnerMatchExecutionCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, ULN {Uln}, Succeeded {Succeeded} ";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
