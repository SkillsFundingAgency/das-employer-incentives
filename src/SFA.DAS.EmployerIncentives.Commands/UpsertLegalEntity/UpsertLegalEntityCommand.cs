using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity
{    
    public class UpsertLegalEntityCommand : ICommand, ILockIdentifier
    {
        public long AccountId { get; private set; }
        public long LegalEntityId { get; private set; }
        public string Name { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LockId { get => $"{nameof(Account)}_{AccountId}"; }

        public UpsertLegalEntityCommand(
            long accountId,
            long legalEntityId,
            string name,
            long accountLegalEntityId)
        {
            AccountId = accountId;
            LegalEntityId = legalEntityId;
            Name = name;
            AccountLegalEntityId = accountLegalEntityId;
        }
    }
}
