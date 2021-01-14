using System;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class ApprenticeApplicationDataRepository : IApprenticeApplicationDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ApprenticeApplicationDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task<List<ApprenticeApplicationDto>> GetList(long accountId, long accountLegalEntityId)
        {
            var accountApplications = from application in _dbContext.ApprenticeshipIncentives
                                      join account in _dbContext.Accounts on application.AccountLegalEntityId equals account.AccountLegalEntityId
                                      where application.AccountId == accountId && application.AccountLegalEntityId == accountLegalEntityId
                                      select new { application, account };

            return await (from accountApplication in accountApplications
                           let dto = new ApprenticeApplicationDto
                           {
                               AccountId = accountApplication.application.AccountId,
                               AccountLegalEntityId = accountApplication.application.AccountLegalEntityId,
                               ApplicationDate = accountApplication.application.SubmittedDate.HasValue ? accountApplication.application.SubmittedDate.Value : DateTime.Now,
                               FirstName = accountApplication.application.FirstName,
                               LastName = accountApplication.application.LastName,
                               ULN = accountApplication.application.ULN,
                               LegalEntityName = accountApplication.account.LegalEntityName,
                               SubmittedByEmail = accountApplication.application.SubmittedByEmail,
                               TotalIncentiveAmount = accountApplication.application.PendingPayments.Sum(x => x.Amount)
                           }
                           select dto).ToListAsync();
        }

        public async Task<Guid?> GetFirstSubmittedApplicationId(long accountLegalEntityId)
        {
            var firstSubmittedApplicationId = await _dbContext.Applications
                .Where(x => x.AccountLegalEntityId == accountLegalEntityId && x.Status == IncentiveApplicationStatus.Submitted)
                .OrderBy(x => x.DateSubmitted)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return firstSubmittedApplicationId;
        }
    }
}
