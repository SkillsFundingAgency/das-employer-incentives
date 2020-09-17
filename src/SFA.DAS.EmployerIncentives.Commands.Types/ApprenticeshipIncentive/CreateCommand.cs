using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class CreateCommand : ICommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; private set; }
        public Guid IncentiveApplicationId { get; private set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public CreateCommand(
            long accountId,
            Guid incentiveApplicationId)
        {         
            AccountId = accountId;
            IncentiveApplicationId = incentiveApplicationId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive CreateCommand for AccountId {AccountId} and IncentiveApplicationId {IncentiveApplicationId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
