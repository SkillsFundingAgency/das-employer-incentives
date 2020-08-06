using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationDataRepository : IIncentiveApplicationDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public IncentiveApplicationDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(IncentiveApplicationModel incentiveApplication)
        {
            await _dbContext.AddAsync(incentiveApplication.Map());
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IncentiveApplicationModel> Get(Guid incentiveApplicationId)
        {
            var incentiveApplication = _dbContext.Applications
                .Include(x => x.Apprenticeships)
                .FirstOrDefault(a => a.Id == incentiveApplicationId);
            if (incentiveApplication != null)
            {
                return await Task.FromResult(incentiveApplication.Map());
            }
            return null;
        }

        public async Task Update(IncentiveApplicationModel incentiveApplication)
        {
            var model = incentiveApplication.Map();
            var existingApplication = _dbContext.Applications.FirstOrDefault(x => x.Id == model.Id);
            if (existingApplication != null)
            {
                _dbContext.Entry(existingApplication).CurrentValues.SetValues(model);
                _dbContext.RemoveRange(existingApplication.Apprenticeships);
                _dbContext.AddRange(model.Apprenticeships);

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
