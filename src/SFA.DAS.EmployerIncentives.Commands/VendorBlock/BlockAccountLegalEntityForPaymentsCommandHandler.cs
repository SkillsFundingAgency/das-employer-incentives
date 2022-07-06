using System;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.VendorBlock
{
    public class BlockAccountLegalEntityForPaymentsCommandHandler : ICommandHandler<BlockAccountLegalEntityForPaymentsCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public BlockAccountLegalEntityForPaymentsCommandHandler(IAccountDomainRepository domainRepository, IDomainEventDispatcher domainEventDispatcher)
        {
            _domainRepository = domainRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task Handle(BlockAccountLegalEntityForPaymentsCommand command, CancellationToken cancellationToken = default)
        {
            var accounts = (await _domainRepository.FindByVendorId(command.VendorId)).ToList();
            
            foreach(var account in accounts)
            {
                account.SetVendorBlockEndDate(command.VendorId, command.VendorBlockEndDate);
                await _domainRepository.Save(account);
            }

            if (accounts.Any())
            {
                var createdEvent = new VendorBlockCreated(command.VendorId,
                    command.VendorBlockEndDate,
                    new ServiceRequest(
                        command.ServiceRequestTaskId,
                        command.ServiceRequestDecisionReference,
                        command.ServiceRequestCreatedDate ?? DateTime.Now));

                await _domainEventDispatcher.Send(createdEvent);
            }
        }
    }

}
