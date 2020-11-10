using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveDataRepository : IApprenticeshipIncentiveDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public ApprenticeshipIncentiveDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            await _dbContext.AddAsync(apprenticeshipIncentive.Map());
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ApprenticeshipIncentiveModel>> FindApprenticeshipIncentivesWithoutPayments()
        {
            var queryResult = (from result in (_dbContext.ApprenticeshipIncentives
                   .Include(x => x.PendingPayments)
                   .Where(x => x.PendingPayments.Count() == 0))
                               let item = result.Map()
                               select item).ToList();

            return await Task.FromResult(queryResult);
        }

        public async Task<ApprenticeshipIncentiveModel> FindByApprenticeshipId(Guid incentiveApplicationApprenticeshipId)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
               .Include(x => x.PendingPayments)
               .FirstOrDefaultAsync(a => a.IncentiveApplicationApprenticeshipId == incentiveApplicationApprenticeshipId);
            if (apprenticeshipIncentive != null)
            {
                return apprenticeshipIncentive.Map();
            }
            return null;
        }

        public async Task<ApprenticeshipIncentiveModel> Get(Guid id)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
                .Include(x => x.PendingPayments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (apprenticeshipIncentive != null)
            {
                return apprenticeshipIncentive.Map();
            }
            return null;
        }

        public async Task Update(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            var model = apprenticeshipIncentive.Map();
            var existingIncentive = await _dbContext.ApprenticeshipIncentives.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existingIncentive != null)
            {
                _dbContext.Entry(existingIncentive).CurrentValues.SetValues(model);
                _dbContext.RemoveRange(existingIncentive.PendingPayments);
                _dbContext.AddRange(model.PendingPayments);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
