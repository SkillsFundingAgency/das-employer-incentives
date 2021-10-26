using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class UpdateIncompleteEarningsCalculationCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; private set; }
        public Guid IncentiveApplicationApprenticeshipId { get; private set; }
        public long ApprenticeshipId { get; private set; }

        public string LockId
        {
            get => $"{nameof(Account)}_{AccountId}";
        }

        public UpdateIncompleteEarningsCalculationCommand(
            long accountId,
            Guid incentiveApplicationApprenticeshipId,
            long apprenticeshipId)
        {
            AccountId = accountId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
            ApprenticeshipId = apprenticeshipId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message =
                    $"ApprenticeshipIncentives UpdateIncompleteEarningsCalculationCommand for AccountId {AccountId}, IncentiveApplicationApprenticeshipId {IncentiveApplicationApprenticeshipId}, and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}