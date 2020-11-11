using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceApplicationsCheckCommandHandler : ICommandHandler<EarningsResilienceApplicationsCheckCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _applicationDomainRepository;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public EarningsResilienceApplicationsCheckCommandHandler(IIncentiveApplicationDomainRepository applicationDomainRepository,
                                                                 IDomainEventDispatcher domainEventDispatcher)
        {
            _applicationDomainRepository = applicationDomainRepository;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task Handle(EarningsResilienceApplicationsCheckCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _applicationDomainRepository.FindIncentiveApplicationsWithoutEarningsCalculations();
            foreach (var application in applications)
            {
                await _domainEventDispatcher.Send(new EarningsCalculationRequired(application.GetModel()));                
            }
        }
    }
}
