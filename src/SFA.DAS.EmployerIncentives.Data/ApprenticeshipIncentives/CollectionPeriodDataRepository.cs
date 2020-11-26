using Microsoft.EntityFrameworkCore;
using NLog.LayoutRenderers;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            var collectionPeriods = await _dbContext.Set<Models.CollectionPeriod>().ToListAsync();
            if (collectionPeriods.Count > 0)
            {
                return collectionPeriods.Map();
            }
            return null;
        }

        public async Task Save(IEnumerable<Domain.ValueObjects.CollectionPeriod> collectionPeriods)
        {
            var models = new Collection<Domain.ValueObjects.CollectionPeriod>(collectionPeriods.ToList()).Map();

            foreach(var model in models)
            {
                var existingCollectionPeriod = await _dbContext.Set<CollectionPeriod>()
                    .FirstOrDefaultAsync(x => x.CalendarYear == model.CalendarYear && x.PeriodNumber == model.PeriodNumber);
                if (existingCollectionPeriod != null)
                {
                    model.Id = existingCollectionPeriod.Id;
                    _dbContext.Entry(existingCollectionPeriod).CurrentValues.SetValues(model);
                }
            }

        }
    }
}
