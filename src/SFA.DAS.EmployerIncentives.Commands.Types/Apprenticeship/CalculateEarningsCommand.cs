using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.Aprenticeship
{
    public class CalculateEarningsCommand : ICommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; private set; }
        public Guid IncentiveClaimApplicationId { get; private set; }
        public long ApprenticeshipId { get; private set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public CalculateEarningsCommand(
            long accountId,
            Guid incentiveClaimApplicationId,
            long apprenticeshipId)
        {
            AccountId = accountId;
            IncentiveClaimApplicationId = incentiveClaimApplicationId;
            ApprenticeshipId = apprenticeshipId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"CalculateEarningsCommand for AccountId {AccountId}, IncentiveClaimApplicationId {IncentiveClaimApplicationId} and ApprenticeshipId {ApprenticeshipId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
