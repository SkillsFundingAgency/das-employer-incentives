using System;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class CollectionPeriodDataRepository : ICollectionPeriodDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public CollectionPeriodDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task<IEnumerable<Domain.ValueObjects.CollectionPeriod>> GetAll()
        {
            var collectionPeriods = await _dbContext.Set<CollectionPeriod>().ToListAsync();
            if (collectionPeriods.Count > 0)
            {
                return collectionPeriods.Map();
            }
            return null;
        }     
    }
}
