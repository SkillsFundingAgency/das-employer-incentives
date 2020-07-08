using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommand : ICommand, ILockIdentifier
    {
        public long AccountId { get; private set; }
        public long AccountLegalEntityId { get; set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public RemoveLegalEntityCommand(
            long accountId,
            long accountLegalEntityId)
        {
            AccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
