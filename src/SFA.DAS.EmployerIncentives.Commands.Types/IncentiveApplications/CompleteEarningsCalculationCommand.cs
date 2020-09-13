using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications
{
    public class CompleteEarningsCalculationCommand : ICommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; private set; }
        public Guid IncentiveApplicationApprenticeshipId { get; private set; }
        public long ApprenticeshipId { get; private set; }
        public Guid ApprenticeshipIncentiveId { get; private set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public CompleteEarningsCalculationCommand(
            long accountId,
            Guid incentiveApplicationApprenticeshipId,
            long apprenticeshipId,
            Guid apprenticeshipIncentiveId)
        {
            AccountId = accountId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
            ApprenticeshipId = apprenticeshipId;
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"IncentiveApplications CompleteEarningsCalculationCommand for AccountId {AccountId}, IncentiveApplicationApprenticeshipId {IncentiveApplicationApprenticeshipId}, ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
