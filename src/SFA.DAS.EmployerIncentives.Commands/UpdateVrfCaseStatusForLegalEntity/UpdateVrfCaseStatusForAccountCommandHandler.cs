using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVrfCaseStatusForAccountCommandHandler : ICommandHandler<UpdateVrfCaseStatusForAccountCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public UpdateVrfCaseStatusForAccountCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateVrfCaseStatusForAccountCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _domainRepository.Find(command.AccountId);
            account.SetVendorRegistrationCaseDetails(command.HashedLegalEntityId, command.CaseId, command.Status, command.LastUpdatedDate);
            await _domainRepository.Save(account);
        }
    }
}