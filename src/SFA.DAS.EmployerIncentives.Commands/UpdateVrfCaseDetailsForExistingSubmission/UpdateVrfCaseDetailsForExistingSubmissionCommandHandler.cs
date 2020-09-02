using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseDetailsForExistingSubmission
{
    public class UpdateVrfCaseDetailsForExistingSubmissionCommandHandler : ICommandHandler<UpdateVrfCaseDetailsForExistingSubmissionCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public UpdateVrfCaseDetailsForExistingSubmissionCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateVrfCaseDetailsForExistingSubmissionCommand command, CancellationToken cancellationToken = default)
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

