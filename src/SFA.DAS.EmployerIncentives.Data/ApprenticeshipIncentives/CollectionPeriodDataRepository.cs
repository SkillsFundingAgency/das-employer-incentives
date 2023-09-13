using Microsoft.EntityFrameworkCore;
using System;
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
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public CollectionPeriodDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task<IEnumerable<Domain.ValueObjects.CollectionCalendarPeriod>> GetAll()
        {
            var collectionPeriods = await _dbContext.Set<Models.CollectionCalendarPeriod>().ToListAsync();
            if (collectionPeriods.Count > 0)
            {
                return collectionPeriods.Map();
            }
            return null;
        }

        public async Task Save(IEnumerable<Domain.ValueObjects.CollectionCalendarPeriod> collectionPeriods)
        {
            var models = new Collection<Domain.ValueObjects.CollectionCalendarPeriod>(collectionPeriods.ToList()).Map();

            foreach(var model in models)
            {
                var existingCollectionPeriod = await _dbContext.Set<CollectionCalendarPeriod>()
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
