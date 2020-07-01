using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommand : ICommand, ILockIdentifier
    {
        public long AccountId { get; private set; }
        public long LegalEntityId { get; private set; }
        public long AccountLegalEntityId { get; set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public RemoveLegalEntityCommand(
            long accountId,
            long legalEntityId,
            long accountLegalEntityId)
        {
            AccountId = accountId;
            LegalEntityId = legalEntityId;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
