using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Serialization;
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
            var tasks = new List<Task>();
            foreach(var earningsValidation in command.EarningsCalculationValidations)
            {
                var incentive = await _domainRepository.FindByApprenticeshipId(earningsValidation.IncentiveApplicationApprenticeshipId);
                if (incentive != null && incentive.PendingPayments.Any())
                {
                    var completeEarningCalculationCommand = new CompleteEarningsCalculationCommand(earningsValidation.AccountId,
                        earningsValidation.IncentiveApplicationApprenticeshipId, earningsValidation.ApprenticeshipId, incentive.Id);

                    tasks.Add(_commandDispatcher.Send(completeEarningCalculationCommand));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
