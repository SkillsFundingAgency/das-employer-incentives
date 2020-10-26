using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.LegalEntityVendorRegistrationForm
{
    public class LegalEntityVendorRegistrationFormQueryRepository : IQueryRepository<LegalEntityVendorRegistrationFormDto>
    {
        private readonly EmployerIncentivesDbContext _context;

        public LegalEntityVendorRegistrationFormQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<LegalEntityVendorRegistrationFormDto> Get(Expression<Func<LegalEntityVendorRegistrationFormDto, bool>> predicate)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalVendorRegistrationFormDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<LegalEntityVendorRegistrationFormDto>> GetList(Expression<Func<LegalEntityVendorRegistrationFormDto, bool>> predicate = null)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalVendorRegistrationFormDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.Account, LegalEntityVendorRegistrationFormDto>> AccountToLegalVendorRegistrationFormDto()
        {
            return x => new LegalEntityVendorRegistrationFormDto
            {
                LegalEntityId = x.LegalEntityId,
                VrfCaseId = x.VrfCaseId,
                VrfCaseStatus = x.VrfCaseStatus,
                VrfVendorId = x.VrfVendorId
            };
        }
    }
}
