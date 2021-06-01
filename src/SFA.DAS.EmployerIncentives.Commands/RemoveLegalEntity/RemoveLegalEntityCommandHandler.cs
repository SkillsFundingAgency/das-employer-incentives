using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommandHandler : ICommandHandler<RemoveLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _accountDomainRepository;

        public RemoveLegalEntityCommandHandler(IAccountDomainRepository accountDomainRepository, 
            CancellationToken cancellationToken = default)
        {
            _accountDomainRepository = accountDomainRepository;
        }

        public async Task Handle(RemoveLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _accountDomainRepository.Find(command.AccountId);

            var legalEntity = account?.GetLegalEntity(command.AccountLegalEntityId);
            if (legalEntity == null)
            {
                // already deleted
                return;
            }

            account.RemoveLegalEntity(legalEntity);
            
            await _accountDomainRepository.Save(account);
        }
    }
}
