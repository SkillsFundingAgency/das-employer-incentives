using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using System;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class AccountDataRepository : IAccountDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public AccountDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
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

        public async Task<IEnumerable<AccountDto>> GetByVrfCaseStatus(string vrfCaseStatus)
        {
            var accountsWithApplications = await (from account in _dbContext.Accounts
                                           join application in _dbContext.Applications
                                           on account.AccountLegalEntityId equals application.AccountLegalEntityId
                                           where account.VrfCaseStatus == vrfCaseStatus
                                           select new Models.Account
                                           {
                                               AccountLegalEntityId = account.AccountLegalEntityId,
                                               HashedLegalEntityId = account.HashedLegalEntityId,
                                               HasSignedIncentivesTerms = account.HasSignedIncentivesTerms,
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
        public async Task UpdatePaidDateForPaymentIds(List<Guid> paymentIds, long accountLegalEntityId, DateTime paidDate)
        {
            var payments = await _dbContext.Payments.Where(x => x.AccountLegalEntityId == accountLegalEntityId).ToListAsync();
            foreach (var paymentId in paymentIds)
            {
                var payment = payments.SingleOrDefault(p => p.Id == paymentId);
                if (payment != null)
                {
                    payment.PaidDate ??= paidDate;
                }
            }
        }



    }
}
