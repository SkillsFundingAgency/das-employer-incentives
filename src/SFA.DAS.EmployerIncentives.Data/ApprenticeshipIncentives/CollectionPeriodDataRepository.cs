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
        private readonly EmployerIncentivesDbContext _dbContext;

        public CollectionPeriodDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
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
