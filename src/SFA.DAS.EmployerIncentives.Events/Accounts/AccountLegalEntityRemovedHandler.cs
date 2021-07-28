using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.Accounts
{
    public class AccountLegalEntityRemovedHandler : IDomainEventHandler<AccountLegalEntityRemoved>
    {
        private readonly IIncentiveApplicationDataRepository _incentiveApplicationDataRepository;
        private readonly ICommandPublisher _commandPublisher;

        public AccountLegalEntityRemovedHandler(IIncentiveApplicationDataRepository incentiveApplicationDataRepository, 
                                                ICommandPublisher commandPublisher)
        {
            _incentiveApplicationDataRepository = incentiveApplicationDataRepository;
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(AccountLegalEntityRemoved @event, CancellationToken cancellationToken = default)
        {
            var applications = await _incentiveApplicationDataRepository.FindApplicationsByAccountLegalEntity(@event.AccountLegalEntityId);

            var withdrawTasks = new List<Task>();
            foreach (var application in applications)
            {
                foreach (var apprenticeship in application.ApprenticeshipModels)
                {
                    var withdrawCommand = new WithdrawCommand(application.AccountId, apprenticeship.Id, WithdrawnBy.Employer);

                    withdrawTasks.Add(_commandPublisher.Publish(withdrawCommand));
                }
            }

            await Task.WhenAll(withdrawTasks);
        }
    }
}
