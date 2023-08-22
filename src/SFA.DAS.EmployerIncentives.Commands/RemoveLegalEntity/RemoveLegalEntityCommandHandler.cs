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
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;

        public RemoveLegalEntityCommandHandler(IAccountDomainRepository accountDomainRepository,
            IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository,
            CancellationToken cancellationToken = default)
        {
            _accountDomainRepository = accountDomainRepository;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
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

            var applications = await _apprenticeshipIncentiveDomainRepository.FindByAccountLegalEntity(command.AccountLegalEntityId);

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
