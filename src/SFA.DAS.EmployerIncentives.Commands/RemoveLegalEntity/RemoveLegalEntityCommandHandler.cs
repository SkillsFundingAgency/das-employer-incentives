using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;

namespace SFA.DAS.EmployerIncentives.Commands.RemoveLegalEntity
{
    public class RemoveLegalEntityCommandHandler : ICommandHandler<RemoveLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _accountDomainRepository;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IIncentiveApplicationDomainRepository _incentiveApplicationDomainRepository;

        public RemoveLegalEntityCommandHandler(IAccountDomainRepository accountDomainRepository, 
            IIncentiveApplicationDomainRepository incentiveApplicationDomainRepository,
            ICommandDispatcher commandDispatcher, 
            CancellationToken cancellationToken = default)
        {
            _accountDomainRepository = accountDomainRepository;
            _incentiveApplicationDomainRepository = incentiveApplicationDomainRepository;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Handle(RemoveLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _accountDomainRepository.Find(command.AccountId);

            var legalEntity = account?.GetLegalEntity(command.AccountLegalEntityId);
            if (legalEntity == null)
            {
                // already deleted
                return;
            }

            account.RemoveLegalEntity(legalEntity);
            
            var applications = await _incentiveApplicationDomainRepository.FindByAccountLegalEntity(command.AccountLegalEntityId);

            var withdrawTasks = new List<Task>();
            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    var withdrawCommand = new WithdrawCommand(application.AccountId, apprenticeship.Id);

                    withdrawTasks.Add(_commandDispatcher.Send(withdrawCommand));
                }
            }

            await Task.WhenAll(withdrawTasks);
            
            await _accountDomainRepository.Save(account);
        }
    }
}
