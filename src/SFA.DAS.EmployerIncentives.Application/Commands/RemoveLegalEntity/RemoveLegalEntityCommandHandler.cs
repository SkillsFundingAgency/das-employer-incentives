using SFA.DAS.EmployerIncentives.Application.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommandHandler : ICommandHandler<RemoveLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public RemoveLegalEntityCommandHandler(IAccountDomainRepository domainRepository, CancellationToken cancellationToken = default)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(RemoveLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _domainRepository.Find(command.AccountId);

            if (account == null)
            {
                // already deleted
                return;
            }

            var legalEntity = account.GetLegalEntity(command.AccountLegalEntityId);
            if (legalEntity == null)
            {
                // already deleted
                return;
            }

            account.RemoveLegalEntity(legalEntity);

            await _domainRepository.Save(account);
        }
    }
}
