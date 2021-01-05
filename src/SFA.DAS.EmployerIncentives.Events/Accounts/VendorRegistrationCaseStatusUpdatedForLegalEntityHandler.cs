using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.Accounts
{
    public class VendorRegistrationCaseStatusUpdatedForLegalEntityHandler : IDomainEventHandler<VendorRegistrationCaseStatusUpdatedForLegalEntity>
    {
        private readonly ICommandPublisher _commandPublisher;

        public VendorRegistrationCaseStatusUpdatedForLegalEntityHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(VendorRegistrationCaseStatusUpdatedForLegalEntity @event, CancellationToken cancellationToken = default)
        {
            var command = new UpdateVendorRegistrationCaseStatusForLegalEntityCommand(@event.HashedLegalEntityId,
                @event.CaseId, @event.Status, @event.LastUpdatedDate);

            await _commandPublisher.Publish(command, cancellationToken);
        }
    }
}
