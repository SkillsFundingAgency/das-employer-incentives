using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.Accounts
{
    public class BankDetailsApprovedForLegalEntityHandler : IDomainEventHandler<BankDetailsApprovedForLegalEntity>
    {
        private readonly ICommandPublisher _commandPublisher;

        public BankDetailsApprovedForLegalEntityHandler(ICommandPublisher commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task Handle(BankDetailsApprovedForLegalEntity @event, CancellationToken cancellationToken = default)
        {
            var command = new AddEmployerVendorIdCommand()
            {
                HashedLegalEntityId = @event.HashedLegalEntityId
            };

            return _commandPublisher.Publish(command);
        }
    }
}
