using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplicationLegalEntity
{
    public class IncentiveApplicationLegalEntityQueryRepository : IQueryRepository<DataTransferObjects.Queries.IncentiveApplicationLegalEntity>
    {
        private class JoinedObject
        {
            public Models.IncentiveApplication Application { get; set; }
            public Models.Account Account { get; set; }
        }

        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public IncentiveApplicationLegalEntityQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<DataTransferObjects.Queries.IncentiveApplicationLegalEntity> Get(Expression<Func<DataTransferObjects.Queries.IncentiveApplicationLegalEntity, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationLegalEntityDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<DataTransferObjects.Queries.IncentiveApplicationLegalEntity>> GetList(Expression<Func<DataTransferObjects.Queries.IncentiveApplicationLegalEntity, bool>> predicate = null)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationLegalEntityDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<JoinedObject, DataTransferObjects.Queries.IncentiveApplicationLegalEntity>> MapToIncentiveApplicationLegalEntityDto()
        {
            return x => new DataTransferObjects.Queries.IncentiveApplicationLegalEntity
            {
                ApplicationId = x.Application.Id,
                ApplicationStatus = x.Application.Status,
                LegalEntityId = x.Account.LegalEntityId,
                VrfCaseId = x.Account.VrfCaseId
            };
        }
    }
}
