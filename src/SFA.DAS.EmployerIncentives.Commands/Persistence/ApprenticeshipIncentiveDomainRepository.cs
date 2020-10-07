using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence
{
    public class ApprenticeshipIncentiveDomainRepository : IApprenticeshipIncentiveDomainRepository
    {
        private readonly IApprenticeshipIncentiveDataRepository _apprenticeshipIncentiveDataRepository;
        private readonly IApprenticeshipIncentiveFactory _apprenticeshipIncentiveFactory;
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public ApprenticeshipIncentiveDomainRepository(
            IApprenticeshipIncentiveDataRepository apprenticeshipIncentiveDataRepository,
            IApprenticeshipIncentiveFactory apprenticeshipIncentiveFactory,
            IDomainEventDispatcher domainEventDispatcher)
        {
            _apprenticeshipIncentiveDataRepository = apprenticeshipIncentiveDataRepository;
            _apprenticeshipIncentiveFactory = apprenticeshipIncentiveFactory;
            _domainEventDispatcher = domainEventDispatcher;
        }

        public async Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> Find(Guid id)
        {
            var application = await _apprenticeshipIncentiveDataRepository.Get(id);
            if (application != null)
            {
                return await Task.FromResult(_apprenticeshipIncentiveFactory.GetExisting(id, application));
            }

            return null;
        }

        public async Task Save(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive aggregate)
        {
            if (aggregate.IsNew)
            {
                await _apprenticeshipIncentiveDataRepository.Add(aggregate.GetModel());
            }
            else
            {
                await _apprenticeshipIncentiveDataRepository.Update(aggregate.GetModel());
            }

            foreach (dynamic domainEvent in aggregate.FlushEvents())
            {
                await _domainEventDispatcher.Send(domainEvent);
            }
        }
    }
}
