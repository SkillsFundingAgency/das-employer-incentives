using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity
{
    public class UpsertLegalEntityCommandHandler : ICommandHandler<UpsertLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;
        private readonly IHashingService _hashingService;

        public UpsertLegalEntityCommandHandler(IAccountDomainRepository domainRepository, IHashingService hashingService)
        {
            _domainRepository = domainRepository;
            _hashingService = hashingService;
        }

        public async Task Handle(UpsertLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _domainRepository.Find(command.AccountId);
            if (account == null)
            {
                account = Account.New(command.AccountId);
            }

            var legalEntity = account.GetLegalEntity(command.AccountLegalEntityId);
            if (account.GetLegalEntity(command.AccountLegalEntityId) != null)
            {
                if(!string.IsNullOrEmpty(legalEntity.HashedLegalEntityId))
                    return; // already created
            
                legalEntity.SetHashedLegalEntityId(_hashingService.HashValue(command.LegalEntityId));
            }
            else
            {
                account.AddLegalEntity(command.AccountLegalEntityId, LegalEntity.New(command.LegalEntityId, command.Name,_hashingService.HashValue(command.LegalEntityId)));
            }

            await _domainRepository.Save(account);
        }
    }
}
