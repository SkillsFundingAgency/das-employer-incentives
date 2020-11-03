using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplicationLegalEntity
{
    public class IncentiveApplicationLegalEntityQueryRepository : IQueryRepository<IncentiveApplicationLegalEntityDto>
    {
        private class JoinedObject
        {
            public Models.IncentiveApplication Application { get; set; }
            public Models.Account Account { get; set; }
        }

        private readonly ApplicationDbContext _context;

        public IncentiveApplicationLegalEntityQueryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IncentiveApplicationLegalEntityDto> Get(Expression<Func<IncentiveApplicationLegalEntityDto, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationLegalEntityDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<IncentiveApplicationLegalEntityDto>> GetList(Expression<Func<IncentiveApplicationLegalEntityDto, bool>> predicate = null)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationLegalEntityDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<JoinedObject, IncentiveApplicationLegalEntityDto>> MapToIncentiveApplicationLegalEntityDto()
        {
            return x => new IncentiveApplicationLegalEntityDto
            {
                ApplicationId = x.Application.Id,
                ApplicationStatus = x.Application.Status,
                LegalEntityId = x.Account.LegalEntityId,
                VrfCaseId = x.Account.VrfCaseId
            };
        }
    }
}
