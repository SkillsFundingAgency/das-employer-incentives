using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Data
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
            var incentiveApplication = _dbContext.Applications.FirstOrDefault(a => a.Id == incentiveApplicationId);
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
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
