using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveQueryRepository : IApprenticeshipIncentiveQueryRepository
    {
        private class JoinedObject
        {
            public Models.ApprenticeshipIncentive Incentive { get; set; }
            public Data.Models.IncentiveApplicationApprenticeship Apprenticeship { get; set; }
        }

        private readonly EmployerIncentivesDbContext _context;

        public ApprenticeshipIncentiveQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<List<ApprenticeshipIncentiveDto>> GetList()
        {
            return _context.Set<Models.ApprenticeshipIncentive>()
                .Join(_context.Set<Data.Models.IncentiveApplicationApprenticeship>(), 
                    ai => ai.IncentiveApplicationApprenticeshipId, iaa => iaa.Id, 
                    (ai, iaa) => new ApprenticeshipIncentiveDto { Id = ai.Id, ApprenticeshipId = ai.ApprenticeshipId, ULN = ai.Uln, UKPRN = iaa.UKPRN })
                .Select(x => x).ToListAsync();
        }
    }
}
