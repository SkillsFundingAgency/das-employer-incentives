using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class UpdateIncompleteEarningsCalculationCommandHandler : ICommandHandler<UpdateIncompleteEarningsCalculationCommand>
    {
        private IApprenticeshipIncentiveDomainRepository _domainRepository;
        private ICommandPublisher _commandPublisher;

        public UpdateIncompleteEarningsCalculationCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository, ICommandPublisher commandPublisher)
        {
            _domainRepository = domainRepository;
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(UpdateIncompleteEarningsCalculationCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incentive = await _domainRepository.FindByApprenticeshipId(command.IncentiveApplicationApprenticeshipId);
            if (incentive.PendingPayments.Any())
            {
                var completeEarningCalculationCommand = new CompleteEarningsCalculationCommand(command.AccountId,
                    command.IncentiveApplicationApprenticeshipId, command.ApprenticeshipId, incentive.Id);

                await _commandPublisher.Publish(completeEarningCalculationCommand);
            }
        }
    }
}
