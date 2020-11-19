using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class AddEmployerVendorIdForLegalEntityCommandHandler : ICommandHandler<AddEmployerVendorIdForLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public AddEmployerVendorIdForLegalEntityCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(AddEmployerVendorIdForLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var accounts = await _domainRepository.GetByHashedLegalEntityId(command.HashedLegalEntityId);

            foreach (var account in accounts)
            {
                account.AddEmployerVendorIdToLegalEntities(command.HashedLegalEntityId, command.EmployerVendorId);
                await _domainRepository.Save(account);
            }
        }
    }
}

