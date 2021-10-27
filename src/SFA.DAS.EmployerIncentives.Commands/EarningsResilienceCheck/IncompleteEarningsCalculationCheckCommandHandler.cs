using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class IncompleteEarningsCalculationCheckCommandHandler : ICommandHandler<IncompleteEarningsCalculationCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public IncompleteEarningsCalculationCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository,
            ICommandDispatcher commandDispatcher)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Handle(IncompleteEarningsCalculationCheckCommand command,  CancellationToken cancellationToken = default(CancellationToken))
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();
            var tasks = new List<Task>();
            foreach (var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    var updateCommand = new UpdateIncompleteEarningsCalculationCommand(application.AccountId,
                        apprenticeship.Id, apprenticeship.ApprenticeshipId);
                    tasks.Add(_commandDispatcher.Send(updateCommand));
                }
            }

            await Task.WhenAll(tasks);
        }
    }

}
