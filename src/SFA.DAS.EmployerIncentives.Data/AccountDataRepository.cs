using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AccountDataRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Update(AccountModel account)
        {
            var existing = await _dbContext.Accounts.Where(a => a.Id == account.Id).ToListAsync();

            foreach (Models.Account item in existing)
            {
                if (!account.LegalEntityModels.Any(i => i.AccountLegalEntityId == item.AccountLegalEntityId))
                {
                    _dbContext.Remove(item);
                }
            }

            foreach (Models.Account item in account.Map())
            {
                var legalEntity = existing.SingleOrDefault(a => a.AccountLegalEntityId == item.AccountLegalEntityId);
                if (legalEntity == null)
                {
                    _dbContext.Add(item);
                }
                else
                {
                    legalEntity.LegalEntityName = item.LegalEntityName;
                    legalEntity.HasSignedIncentivesTerms = item.HasSignedIncentivesTerms;
                    legalEntity.VrfCaseId = item.VrfCaseId;
                    legalEntity.VrfCaseStatus = item.VrfCaseStatus;
                    legalEntity.VrfVendorId = item.VrfVendorId;
                    legalEntity.VrfCaseStatusLastUpdatedDateTime = item.VrfCaseStatusLastUpdatedDateTime;
                    legalEntity.HashedLegalEntityId = item.HashedLegalEntityId;
                }
            }
        }

        public async Task Add(AccountModel account)
        {
            await _dbContext.AddRangeAsync(account.Map());
        }

        public async Task<AccountModel> Find(long accountId)
        {
            var accounts = await _dbContext.Accounts.Where(a => a.Id == accountId).ToListAsync();
            return accounts?.MapSingle();
        }

        public async Task<IEnumerable<AccountModel>> GetByHashedLegalEntityId(string hashedLegalEntityId)
        {
            var accountIds = _dbContext.Accounts.Where(y => y.HashedLegalEntityId == hashedLegalEntityId).Select(z => z.Id).Distinct();
            var accounts = await _dbContext.Accounts.Where(x => accountIds.Contains(x.Id)).ToListAsync();
            return accounts?.Map();
        }
    }
}
