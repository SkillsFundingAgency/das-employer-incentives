using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForLegalEntity
{
    public class UpdateVrfCaseDetailsForLegalEntityCommandHandler : ICommandHandler<UpdateVrfCaseDetailsForLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public UpdateVrfCaseDetailsForLegalEntityCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateVrfCaseDetailsForLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var accounts = await _domainRepository.GetByLegalEntityId(command.LegalEntityId);

            foreach (var account in accounts)
            {
                account.UpdateVendorRegistrationFormDetails(command.LegalEntityId, command.CaseId, command.VendorId, command.Status);

                await _domainRepository.Save(account);
            }
        }
    }
}

