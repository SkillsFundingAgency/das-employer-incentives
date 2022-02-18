using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Decorators
{
    public class LearnerServiceWithCache : ILearnerService
    {
        private readonly ILearnerService _service;
        private readonly ILearnerDomainRepository _learnerDomainRepository;
        private readonly IDateTimeService _dateTimeService;
        private readonly bool _isCaching;
        private readonly uint _cacheIntervalInMinutes;

        public LearnerServiceWithCache(
            ILearnerService service,
            ILearnerDomainRepository learnerDomainRepository,
            IOptions<ApplicationSettings> options,
            IDateTimeService dateTimeService)
        {
            var cacheInterval = options?.Value.LearnerServiceCacheIntervalInMinutes;
            if(!string.IsNullOrWhiteSpace(cacheInterval))
            {
                _isCaching = true;
                _cacheIntervalInMinutes = Convert.ToUInt32(cacheInterval);
            }
            _service = service;
            _learnerDomainRepository = learnerDomainRepository;
            _dateTimeService = dateTimeService;
        }

        public async Task<Learner> Refresh(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive)
        {
            if(_isCaching)
            {
                var learner = await _learnerDomainRepository.Get(incentive);
                if (learner != null 
                    && learner.LastRefreshed.HasValue 
                    && learner.LastRefreshed.Value.AddMinutes(_cacheIntervalInMinutes) > _dateTimeService.UtcNow())
                {
                    return learner;
                }
            }

            return await _service.Refresh(incentive);
        }
    }
}
