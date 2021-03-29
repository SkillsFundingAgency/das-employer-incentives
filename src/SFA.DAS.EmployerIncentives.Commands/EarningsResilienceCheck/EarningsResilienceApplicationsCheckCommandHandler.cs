using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Events;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceApplicationsCheckCommandHandler : ICommandHandler<EarningsResilienceApplicationsCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public EarningsResilienceApplicationsCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository, IDomainEventDispatcher domainEventDispatcher)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _domainEventDispatcher = domainEventDispatcher;
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
            }

            await Task.WhenAll(tasks);
        }
    }
}
