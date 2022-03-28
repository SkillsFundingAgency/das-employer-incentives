using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Enums;

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
            return _context.ApprenticeshipIncentives
                .Where(a => a.Status != IncentiveStatus.Withdrawn)
                .Select(ApprenticeshipIncentiveToApprenticeshipIncentiveDto()).ToListAsync();
        }

        public Task<List<ApprenticeshipIncentive>> GetList(Expression<Func<ApprenticeshipIncentive, bool>> predicate = null)
        {
            return _context.Set<ApprenticeshipIncentive>()
                .Where(predicate)
                .ToListAsync();
        }

        public Task<List<ApprenticeshipIncentiveDto>> GetDtoList(Expression<Func<ApprenticeshipIncentive, bool>> predicate = null)
        {
            return _context.Set<ApprenticeshipIncentive>()
                .Where(predicate)
                .Select(ApprenticeshipIncentiveToApprenticeshipIncentiveDto())
                .ToListAsync();
        }

        public Task<ApprenticeshipIncentive> Get(Expression<Func<ApprenticeshipIncentive, bool>> predicate)
        {
            return _context
                .Set<ApprenticeshipIncentive>()
                .SingleOrDefaultAsync(predicate);
        }
        

        public Task<ApprenticeshipIncentive> Get(Expression<Func<ApprenticeshipIncentive, bool>> predicate, bool includePayments = false)
        {
            if(includePayments)
            {
                return _context
                .Set<ApprenticeshipIncentive>()
                .Include(a => a.Payments)
                .SingleOrDefaultAsync(predicate);
            }
            else
            {
                return Get(predicate);
            }
        }

        private Expression<Func<ApprenticeshipIncentive, ApprenticeshipIncentiveDto>> ApprenticeshipIncentiveToApprenticeshipIncentiveDto()
        {
            return x => new ApprenticeshipIncentiveDto
            {
                Id = x.Id,
                ApprenticeshipId = x.ApprenticeshipId,
                ULN = x.ULN,
                UKPRN = x.UKPRN,
                CourseName = x.CourseName,
                StartDate = x.StartDate,
                FirstName = x.FirstName,
                LastName = x.LastName
            };
        }
    }
}
