using System;
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
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public ApprenticeshipIncentiveQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<List<ApprenticeshipIncentiveDto>> GetList()
        {
            return _context.Set<Models.ApprenticeshipIncentive>()
                .Select(x => new ApprenticeshipIncentiveDto { Id = x.Id, ApprenticeshipId = x.ApprenticeshipId, ULN = x.ULN, UKPRN = x.UKPRN }).ToListAsync();
        }
    }
}
