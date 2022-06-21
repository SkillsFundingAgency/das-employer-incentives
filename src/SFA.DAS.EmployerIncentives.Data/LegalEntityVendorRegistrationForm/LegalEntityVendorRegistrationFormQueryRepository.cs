using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.LegalEntityVendorRegistrationForm
{
    public class LegalEntityVendorRegistrationFormQueryRepository : IQueryRepository<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm>
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public LegalEntityVendorRegistrationFormQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm> Get(Expression<Func<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm, bool>> predicate)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalVendorRegistrationFormDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm>> GetList(Expression<Func<DataTransferObjects.Queries.LegalEntityVendorRegistrationForm, bool>> predicate = null)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalVendorRegistrationFormDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.Account, DataTransferObjects.Queries.LegalEntityVendorRegistrationForm>> AccountToLegalVendorRegistrationFormDto()
        {
            return x => new DataTransferObjects.Queries.LegalEntityVendorRegistrationForm
            {
                LegalEntityId = x.LegalEntityId,
                VrfCaseId = x.VrfCaseId,
                VrfCaseStatus = x.VrfCaseStatus,
                VrfVendorId = x.VrfVendorId
            };
        }
    }
}
