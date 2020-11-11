using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplicationApprenticeship
{
    public class IncentiveApplicationApprenticeshipQueryRepository : IQueryRepository<Models.IncentiveApplicationApprenticeship>
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public IncentiveApplicationApprenticeshipQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<Models.IncentiveApplicationApprenticeship> Get(Expression<Func<Models.IncentiveApplicationApprenticeship, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplicationApprenticeship>().SingleOrDefaultAsync(predicate);
        }

        public Task<List<Models.IncentiveApplicationApprenticeship>> GetList(Expression<Func<Models.IncentiveApplicationApprenticeship, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplicationApprenticeship>().Where(predicate).ToListAsync();
        }
    }
}
