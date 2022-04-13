using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Data.Account
{
    public class AccountQueryRepository : IQueryRepository<LegalEntity>
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public AccountQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<LegalEntity> Get(Expression<Func<LegalEntity, bool>> predicate)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalEntityDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<LegalEntity>> GetList(Expression<Func<LegalEntity, bool>> predicate = null)
        {
            return _context.Set<Models.Account>()
                .Select(AccountToLegalEntityDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.Account, LegalEntity>> AccountToLegalEntityDto()
        {
            return x => new LegalEntity
            {
                AccountId = x.Id,
                AccountLegalEntityId = x.AccountLegalEntityId,
                LegalEntityId = x.LegalEntityId,
                LegalEntityName = x.LegalEntityName,
                VrfVendorId = x.VrfVendorId,
                VrfCaseStatus = x.VrfCaseStatus,
                HashedLegalEntityId = x.HashedLegalEntityId,
                SignedAgreementVersion = x.SignedAgreementVersion,
                IsAgreementSigned = x.SignedAgreementVersion.HasValue && x.SignedAgreementVersion >= Phase3Incentive.MinimumAgreementVersion(),
                BankDetailsRequired = String.IsNullOrWhiteSpace(x.VrfCaseStatus)
            };
        }

    }
}