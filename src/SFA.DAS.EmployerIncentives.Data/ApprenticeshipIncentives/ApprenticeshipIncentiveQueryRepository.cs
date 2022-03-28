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
using ApprenticeshipIncentive = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives.ApprenticeshipIncentive;

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

        public Task<List<ApprenticeshipIncentive>> GetList()
        {
            return _context.ApprenticeshipIncentives
                .Where(a => a.Status != IncentiveStatus.Withdrawn)
                .Select(ApprenticeshipIncentiveToApprenticeshipIncentiveDto()).ToListAsync();
        }

        public Task<List<Models.ApprenticeshipIncentive>> GetList(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate = null)
        {
            return _context.Set<Models.ApprenticeshipIncentive>()
                .Where(predicate)
                .ToListAsync();
        }

        public Task<List<ApprenticeshipIncentive>> GetDtoList(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate = null)
        {
            return _context.Set<Models.ApprenticeshipIncentive>()
                .Where(predicate)
                .Select(ApprenticeshipIncentiveToApprenticeshipIncentiveDto())
                .ToListAsync();
        }

        public Task<Models.ApprenticeshipIncentive> Get(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate)
        {
            return _context
                .Set<Models.ApprenticeshipIncentive>()
                .SingleOrDefaultAsync(predicate);
        }
        

        public Task<Models.ApprenticeshipIncentive> Get(Expression<Func<Models.ApprenticeshipIncentive, bool>> predicate, bool includePayments = false)
        {
            if(includePayments)
            {
                return _context
                .Set<Models.ApprenticeshipIncentive>()
                .Include(a => a.Payments)
                .SingleOrDefaultAsync(predicate);
            }
            else
            {
                return Get(predicate);
            }
        }

        private Expression<Func<Models.ApprenticeshipIncentive, ApprenticeshipIncentive>> ApprenticeshipIncentiveToApprenticeshipIncentiveDto()
        {
            return x => new ApprenticeshipIncentive
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
