﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Events;

namespace SFA.DAS.EmployerIncentives.Events.Accounts
{
    public class AccountLegalEntityRemovedHandler : IDomainEventHandler<AccountLegalEntityRemoved>
    {
        private readonly IIncentiveApplicationDataRepository _incentiveApplicationDataRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public AccountLegalEntityRemovedHandler(IIncentiveApplicationDataRepository incentiveApplicationDataRepository, 
                                                ICommandDispatcher commandDispatcher)
        {
            _incentiveApplicationDataRepository = incentiveApplicationDataRepository;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Handle(AccountLegalEntityRemoved @event, CancellationToken cancellationToken = default)
        {
            var applications = await _incentiveApplicationDataRepository.FindApplicationsByAccountLegalEntity(@event.AccountLegalEntityId);

            var withdrawTasks = new List<Task>();
            foreach (var application in applications)
            {
                foreach (var apprenticeship in application.ApprenticeshipModels)
                {
                    var withdrawCommand = new WithdrawCommand(application.AccountId, apprenticeship.Id);

                    withdrawTasks.Add(_commandDispatcher.Send(withdrawCommand));
                }
            }

            await Task.WhenAll(withdrawTasks);
        }
    }
}
