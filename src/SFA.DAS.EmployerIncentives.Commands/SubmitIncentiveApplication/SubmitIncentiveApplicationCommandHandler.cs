using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Services;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class SubmitIncentiveApplicationCommandHandler : ICommandHandler<SubmitIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IEventPublisher _eventPublisher;

        public SubmitIncentiveApplicationCommandHandler(IIncentiveApplicationDomainRepository domainRepository, IEventPublisher eventPublisher)
        {
            _domainRepository = domainRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task Handle(SubmitIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            application.Submit(command.DateSubmitted, command.SubmittedByEmail, command.SubmittedByName);

            await _domainRepository.Save(application);

            await _eventPublisher.Publish(new EmployerIncentiveClaimSubmittedEvent
            {
                AccountId = command.AccountId,
                IncentiveClaimApplicationId = command.IncentiveApplicationId
            });
        }
    }
}
