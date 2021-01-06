using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class WithdrawCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }
        public Guid IncentiveApplicationApprenticeshipId { get; }
        
        public WithdrawCommand(
            long accountId,
            Guid incentiveApplicationApprenticeshipId)
        {
            AccountId = accountId;
            IncentiveApplicationApprenticeshipId = incentiveApplicationApprenticeshipId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message =
                    $"ApprenticeshipIncentive WithdrawCommand for AccountId {AccountId}, " +
                    $"and IncentiveApplicationApprenticeshipId {IncentiveApplicationApprenticeshipId}";
                
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }

    }
}
