using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateVrfCaseStatusForLegalEntity
{
    public class UpdateVendorRegistrationCaseStatusCommandHandler : ICommandHandler<UpdateVendorRegistrationCaseStatusCommand>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IAccountDomainRepository _domainRepository;

        public UpdateVendorRegistrationCaseStatusCommandHandler(ICommandPublisher commandPublisher, IAccountDomainRepository domainRepository)
        {
            _commandPublisher = commandPublisher;
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateVendorRegistrationCaseStatusCommand command, CancellationToken cancellationToken = default)
        {
            var accounts = await _domainRepository.GetByHashedLegalEntityId(command.HashedLegalEntityId);

            var tasks = accounts.Select(account =>
                _commandPublisher.Publish(
                    new UpdateVrfCaseStatusForAccountCommand(
                        account.Id,
                        command.HashedLegalEntityId,
                        command.CaseId,
                        command.Status,
                        command.CaseStatusLastUpdatedDate),
                    cancellationToken));

            await Task.WhenAll(tasks);
        }
    }
}

