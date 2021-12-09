using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId)
        {
            var application = await _apprenticeshipIncentiveDataRepository.FindByApprenticeshipId(incentiveApplicationApprenticeshipId);
            if (application != null)
            {
                return await Task.FromResult(_apprenticeshipIncentiveFactory.GetExisting(application.Id, application));
            }

            return null;
        }

        public async Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByEmploymentCheckId(Guid correlationId)
        {
            var apprenticeshipIncentiveModel = await _apprenticeshipIncentiveDataRepository.FindApprenticeshipIncentiveByEmploymentCheckId(correlationId);

            if(apprenticeshipIncentiveModel == null)
            {
                return null;
            }

            return _apprenticeshipIncentiveFactory.GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);
        }

        public async Task<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive> FindByUlnWithinAccountLegalEntity(long uln, long accountLegalEntityId)
        {
            var apprenticeships = await _apprenticeshipIncentiveDataRepository.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(uln, accountLegalEntityId);

            switch (apprenticeships.Count)
            {
                case 0:
                    return null;
                case 1:
                    return _apprenticeshipIncentiveFactory.GetExisting(apprenticeships[0].Id, apprenticeships[0]); 
                default:
                    throw new InvalidIncentiveException($"Found duplicate ULNs {uln} within account legal entity {accountLegalEntityId}");
            }
        }

        public async Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindIncentivesWithoutPendingPayments()
        {
            var incentives = await _apprenticeshipIncentiveDataRepository.FindApprenticeshipIncentivesWithoutPendingPayments();
            return (from incentive in incentives
                    select _apprenticeshipIncentiveFactory.GetExisting(incentive.Id, incentive)).ToList();
        }

        public async Task<List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>> FindIncentivesWithLearningFound()
        {
            var incentives = await _apprenticeshipIncentiveDataRepository.FindIncentivesWithLearningFound();
            return (from incentive in incentives
                select _apprenticeshipIncentiveFactory.GetExisting(incentive.Id, incentive)).ToList();
        }

        public async Task Save(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive aggregate)
        {
            if (aggregate.IsNew)
            {
                await _apprenticeshipIncentiveDataRepository.Add(aggregate.GetModel());
            }
            else if(aggregate.IsDeleted)
            {
                await _apprenticeshipIncentiveDataRepository.Delete(aggregate.GetModel());
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
