using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Persistence.Decorators
{
    public class DomainRepositoryWithAggregateLogging<EntityId, T> : IDomainRepository<EntityId, T> where T : IAggregateRoot
    {
        private readonly IDomainRepository<EntityId, T> _domainRepository;
        private readonly ILogger<T> _log;

        public DomainRepositoryWithAggregateLogging(
            IDomainRepository<EntityId, T> domainRepository,
            ILoggerFactory loggerFactory)
        {
            _domainRepository = domainRepository;
            _log = loggerFactory.CreateLogger<T>();
        }

        public async Task<T> Find(EntityId id)
        {            
            var entity = await _domainRepository.Find(id);

            if(entity is ILogger)
            {
                var entityLog = (entity as ILogWriter).Log;
                _log.LogInformation($"Retrieved entity '{typeof(T)}' {entityLog.OnProcessing?.Invoke()}");
            }

            return entity;
        }

        public async Task Save(T aggregate)
        {
            if (aggregate is ILogWriter)
            {
                var entityLog = (aggregate as ILogWriter).Log;

                try
                {
                    _log.LogInformation($"Saving '{typeof(T)}'  {entityLog.OnProcessed?.Invoke()}");

                    await _domainRepository.Save(aggregate);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, $"Error saving '{typeof(T)}' : {entityLog.OnError?.Invoke()}");

                    throw;
                }
            }
            else
            {
                await _domainRepository.Save(aggregate);
            }
        }
    }
}
