using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.HashingService;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class MinimumAgreementVersionChangedNotificationHandler : IDomainEventHandler<MinimumAgreementVersionChanged>
    {
        private readonly ICommandPublisher _commandPublisher;
        private readonly IQueryRepository<LegalEntityDto> _accountQueryRepository;
        private readonly IHashingService _hashingService;

        public MinimumAgreementVersionChangedNotificationHandler(ICommandPublisher commandPublisher,
            IQueryRepository<LegalEntityDto> accountQueryRepository, IHashingService hashingService)
        {
            _commandPublisher = commandPublisher;
            _accountQueryRepository = accountQueryRepository;
            _hashingService = hashingService;
        }

        public async Task Handle(MinimumAgreementVersionChanged @event, CancellationToken cancellationToken = default)
        {
            var account = await _accountQueryRepository.Get(a => 
                a.AccountId == @event.Model.Account.Id && a.AccountLegalEntityId == @event.Model.Account.AccountLegalEntityId);
            
            var command = new NotifyNewAgreementRequiredCommand(_hashingService.HashValue(account.AccountId), account.LegalEntityName);

            await _commandPublisher.Publish(command);
        }
    }
}
