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
            var incentiveApplication = _dbContext.Applications.AsNoTracking().FirstOrDefault(a => a.Id == incentiveApplicationId);
            if (incentiveApplication != null)
            {
                return await Task.FromResult(incentiveApplication.Map());
            }
            return null;
        }

        public async Task Update(IncentiveApplicationModel incentiveApplication)
        {
            _dbContext.Update(incentiveApplication.Map());
            await _dbContext.SaveChangesAsync();
        }
    }
}
