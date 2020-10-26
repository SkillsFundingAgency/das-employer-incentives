using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
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

        public async Task<ApprenticeshipIncentiveModel> Get(Guid id)
        {
            var apprenticeshipIncentive = await _dbContext.ApprenticeshipIncentives
                .Include(x => x.PendingPayments)
                .FirstOrDefaultAsync(a => a.Id == id);
            return apprenticeshipIncentive?.Map();
        }

        public async Task Update(ApprenticeshipIncentiveModel apprenticeshipIncentive)
        {
            var model = apprenticeshipIncentive.Map();
            var existingIncentive = await _dbContext.ApprenticeshipIncentives.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existingIncentive != null)
            {
                _dbContext.Entry(existingIncentive).CurrentValues.SetValues(model);
                _dbContext.RemoveRange(existingIncentive.PendingPayments);
                await _dbContext.AddRangeAsync(model.PendingPayments);

                foreach (var payment in existingIncentive.PendingPayments) // TODO: Work out why duplicates
                {
                    _dbContext.RemoveRange(payment.ValidationResults); // TODO: why not fetched
                }
                foreach (var payment in model.PendingPayments)
                {
                    await _dbContext.AddRangeAsync(payment.ValidationResults);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
