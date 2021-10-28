using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class ValidateIncompleteEarningsCalculationCommandHandler : ICommandHandler<ValidateIncompleteEarningsCalculationCommand>
    {
        private IApprenticeshipIncentiveDomainRepository _domainRepository;
        private ICommandDispatcher _commandDispatcher;

        public ValidateIncompleteEarningsCalculationCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, ICommandDispatcher commandPublisher)
        {
            _domainRepository = domainRepository;
            _commandDispatcher = commandPublisher;
        }

        public async Task Handle(ValidateIncompleteEarningsCalculationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentive = await _domainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if (incentive != null && incentive.PendingPayments.Any())
            {
                var completeEarningCalculationCommand = new CompleteEarningsCalculationCommand(command.AccountId,
                    command.IncentiveApplicationApprenticeshipId, command.ApprenticeshipId, incentive.Id);

                await _commandDispatcher.Send(completeEarningCalculationCommand);
            }
        }
    }
}
