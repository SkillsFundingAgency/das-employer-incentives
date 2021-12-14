using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SetSuccessfulLearnerMatch
{
    public class SetSuccessfulLearnerMatchCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
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
        public LogWithArgs Log
        {
            get
            {
                var message = "SuccessfulLearnerMatch for apprenticeship incentive id {apprenticeshipIncentiveId}, ULN {uln}, Succeeded: {succeeded}";

                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>( "Setting " + message, new object[] { ApprenticeshipIncentiveId, Uln, Succeeded }),
                    OnProcessed = () => new Tuple<string, object[]>("Set " + message, new object[] { ApprenticeshipIncentiveId, Uln, Succeeded }),
                    OnError = () => new Tuple<string, object[]>("Error setting " + message, new object[] { ApprenticeshipIncentiveId, Uln, Succeeded })
                };
            }
        }
    }
}
