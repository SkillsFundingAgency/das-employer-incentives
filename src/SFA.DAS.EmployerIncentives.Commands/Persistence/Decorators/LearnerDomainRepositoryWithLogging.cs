using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence.Decorators
{
    public class LearnerDomainRepositoryWithLogging : ILearnerDomainRepository
    {
        private readonly ILearnerDomainRepository _domainRepository;
        private readonly DomainRepositoryWithAggregateLogging<Guid, Learner> _aggregateLogger;

        public LearnerDomainRepositoryWithLogging(
            ILearnerDomainRepository domainRepository,
            ILoggerFactory loggerFactory)
        {
            _domainRepository = domainRepository;
            _aggregateLogger =
                new DomainRepositoryWithAggregateLogging<Guid, Learner>(_domainRepository, loggerFactory);
        }

        public Task<Learner> Find(Guid id)
        {
            return _aggregateLogger.Find(id);
        }

        public Task<Learner> Get(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            return _domainRepository.Get(incentive);
        }

        public Task Save(Learner aggregate)
        {
            return _aggregateLogger.Save(aggregate);
        }
    }
}