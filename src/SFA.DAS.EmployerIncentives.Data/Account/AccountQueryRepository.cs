using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.Account
{
    public class AccountQueryRepository : IQueryRepository<LegalEntityDto>
    {
        private readonly EmployerIncentivesDbContext _context;

        public AccountQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<LegalEntityDto> Get(Expression<Func<LegalEntityDto, bool>> predicate)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalEntityDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<LegalEntityDto>> GetList(Expression<Func<LegalEntityDto, bool>> predicate = null)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalEntityDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.Account, LegalEntityDto>> AccountToLegalEntityDto()
        {
            return x => new LegalEntityDto
            {
                AccountId = x.Id,
                AccountLegalEntityId = x.AccountLegalEntityId,
                LegalEntityId = x.LegalEntityId,
                LegalEntityName = x.LegalEntityName,
                HasSignedIncentivesTerms = x.HasSignedIncentivesTerms,
                VrfCaseStatus = x.VrfCaseStatus
            };
        }
    }
}