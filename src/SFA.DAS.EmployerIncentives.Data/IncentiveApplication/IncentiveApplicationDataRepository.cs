using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
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
        }

        public async Task<IncentiveApplicationModel> Get(Guid incentiveApplicationId)
        {
            var incentiveApplication = await _dbContext.Applications
                .Include(x => x.Apprenticeships)
                .FirstOrDefaultAsync(a => a.Id == incentiveApplicationId);
            if (incentiveApplication != null)
            {
                return incentiveApplication.Map();
            }
            return null;
        }

        public async Task Update(IncentiveApplicationModel incentiveApplication)
        {
            var model = incentiveApplication.Map();
            var existingApplication = await _dbContext.Applications.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (existingApplication != null)
            {
                _dbContext.Entry(existingApplication).CurrentValues.SetValues(model);
                _dbContext.RemoveRange(existingApplication.Apprenticeships);
                _dbContext.AddRange(model.Apprenticeships);
            }
        }
    }
}
