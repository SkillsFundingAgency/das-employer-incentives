using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Types.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceApplicationsCheckCommandHandler : ICommandHandler<EarningsResilienceApplicationsCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ICommandPublisher _commandPublisher;

        public EarningsResilienceApplicationsCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository, IDomainEventDispatcher domainEventDispatcher,
            ICommandPublisher commandPublisher)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _domainEventDispatcher = domainEventDispatcher;
            _commandPublisher = commandPublisher;
        }

        public async Task Handle(EarningsResilienceApplicationsCheckCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();
            var tasks = new List<Task>();
            foreach (var application in applications)
            {
                application.Resubmit();
                foreach (dynamic domainEvent in application.FlushEvents())
                {
                    tasks.Add(_domainEventDispatcher.Send(domainEvent, cancellationToken));
                }
                foreach (var apprenticeship in application.Apprenticeships)
                {
                    var completeEarningCalculationCommand = new CompleteEarningsCalculationCommand(application.AccountId,
                        apprenticeship.Id, apprenticeship.ApprenticeshipId, application.Id);
                    tasks.Add(_commandPublisher.Publish(completeEarningCalculationCommand));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
