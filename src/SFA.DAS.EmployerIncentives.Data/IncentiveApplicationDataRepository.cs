using System.Threading.Tasks;
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
    }
}
