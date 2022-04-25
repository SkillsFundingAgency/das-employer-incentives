using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommandHandler : ICommandHandler<RemoveLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _accountDomainRepository;
        private readonly IIncentiveApplicationDomainRepository _incentiveApplicationRepository;

        public RemoveLegalEntityCommandHandler(IAccountDomainRepository accountDomainRepository,
            IIncentiveApplicationDomainRepository incentiveApplicationRepository,
            CancellationToken cancellationToken = default)
        {
            _accountDomainRepository = accountDomainRepository;
            _incentiveApplicationRepository = incentiveApplicationRepository;
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

            var applications = await _incentiveApplicationRepository.FindByAccountLegalEntity(legalEntity.Id);

            if (applications != null && applications.Any())
            {
                account.MarkLegalEntityRemoved(legalEntity);
            }
            else
            {
                account.RemoveLegalEntity(legalEntity);
            }

            await _accountDomainRepository.Save(account);
        }
    }
}
