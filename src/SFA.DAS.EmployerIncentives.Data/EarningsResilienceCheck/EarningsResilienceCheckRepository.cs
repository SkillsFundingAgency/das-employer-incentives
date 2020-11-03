using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck
{
    public class EarningsResilienceCheckRepository : IEarningsResilienceCheckRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public EarningsResilienceCheckRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Guid>> GetApplicationsWithoutEarningsCalculations()
        {
            var earningsCheckApplications = new List<Guid>();

            var applicationsWithoutApprenticeshipIncentives = await (from applications in _dbContext.Applications
                                                                     join apprenticeships in _dbContext.ApplicationApprenticeships
                                                                     on applications.Id equals apprenticeships.IncentiveApplicationId
                                                                     where apprenticeships.EarningsCalculated == false
                                                                     && applications.Status == Enums.IncentiveApplicationStatus.Submitted
                                                                     select new { applications.Id }).ToListAsync();

            earningsCheckApplications.AddRange(applicationsWithoutApprenticeshipIncentives.Select(application => application.Id));

            return await Task.FromResult(earningsCheckApplications);
        }

        public async Task<IncentiveApplicationModel> GetApplicationDetail(Guid applicationId)
        {
            var incentiveApplication = await _dbContext.Applications
               .Include(x => x.Apprenticeships)
               .FirstOrDefaultAsync(a => a.Id == applicationId);
            if (incentiveApplication != null)
            {
                return incentiveApplication.Map();
            }
            return null;
        }
    }
}
