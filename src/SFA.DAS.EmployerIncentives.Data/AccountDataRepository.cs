using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public AccountDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _dbContext = dbContext.Value;
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
                    legalEntity.SignedAgreementVersion = item.SignedAgreementVersion;
                    legalEntity.VrfCaseId = item.VrfCaseId;
                    legalEntity.VrfCaseStatus = item.VrfCaseStatus;
                    legalEntity.VrfVendorId = item.VrfVendorId;
                    legalEntity.VrfCaseStatusLastUpdatedDateTime = item.VrfCaseStatusLastUpdatedDateTime;
                    legalEntity.HashedLegalEntityId = item.HashedLegalEntityId;
                    legalEntity.VendorBlockEndDate = item.VendorBlockEndDate;
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

        public async Task<IEnumerable<DataTransferObjects.Account>> GetByVrfCaseStatus(string vrfCaseStatus)
        {
            var accountsWithApplications = await (from account in _dbContext.Accounts
                                           join application in _dbContext.Applications
                                           on account.AccountLegalEntityId equals application.AccountLegalEntityId
                                           where account.VrfCaseStatus == vrfCaseStatus
                                           select new Models.Account
                                           {
                                               AccountLegalEntityId = account.AccountLegalEntityId,
                                               HashedLegalEntityId = account.HashedLegalEntityId,
                                               SignedAgreementVersion = account.SignedAgreementVersion,
                                               Id = account.Id,
                                               LegalEntityId = account.LegalEntityId,
                                               LegalEntityName = account.LegalEntityName,
                                               VrfCaseId = account.VrfCaseId,
                                               VrfCaseStatus = account.VrfCaseStatus,
                                               VrfCaseStatusLastUpdatedDateTime = account.VrfCaseStatusLastUpdatedDateTime,
                                               VrfVendorId = account.VrfVendorId
                                           }).ToListAsync();

            return accountsWithApplications?.MapDto();
        }        

        public async Task<DateTime?> GetLatestVendorRegistrationCaseUpdateDateTime()
        {
            return await _dbContext.Accounts.MaxAsync(a => a.VrfCaseStatusLastUpdatedDateTime);
        }

        public async Task<IEnumerable<AccountModel>> FindByVendorId(string vendorId)
        {
            var accounts = new List<Models.Account>();

            var accountsForVendorId = await _dbContext.Accounts.Where(x => x.VrfVendorId == vendorId).ToArrayAsync();
            if (accountsForVendorId.Any())
            {
                var accountId = accountsForVendorId.FirstOrDefault().Id;
                var otherAccountLegalEntities = await _dbContext.Accounts.Where(x => x.Id == accountId && x.VrfVendorId != vendorId).ToArrayAsync();

                accounts.AddRange(accountsForVendorId);
                if (otherAccountLegalEntities.Any())
                {
                    accounts.AddRange(otherAccountLegalEntities);
                }
            }

            return accounts?.Map();
        }
    }
}
