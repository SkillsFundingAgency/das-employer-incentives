using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVendorRegistrationCaseStatusForLegalEntityCommandHandler : ICommandHandler<UpdateVendorRegistrationCaseStatusForLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public UpdateVendorRegistrationCaseStatusForLegalEntityCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateVendorRegistrationCaseStatusForLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var accounts = await _domainRepository.GetByHashedLegalEntityId(command.HashedLegalEntityId);

            foreach (var account in accounts)
            {
                account.SetVendorRegistrationCaseDetails(command.HashedLegalEntityId, command.CaseId, command.Status, command.LastUpdatedDate);
                await _domainRepository.Save(account);
            }
        }
    }
}