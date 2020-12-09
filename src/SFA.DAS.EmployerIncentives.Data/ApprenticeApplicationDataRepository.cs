using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class ApprenticeApplicationDataRepository : IApprenticeApplicationDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public ApprenticeApplicationDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ApprenticeApplicationDto>> GetList(long accountId, long accountLegalEntityId)
        {
            var accountApplications = from application in _dbContext.Applications
                                      join account in _dbContext.Accounts
                                      on application.AccountLegalEntityId equals account.AccountLegalEntityId
                                      join apprentice in _dbContext.ApplicationApprenticeships
                                      on application.Id equals apprentice.IncentiveApplicationId
                                      where application.AccountId == accountId
                                      && application.AccountLegalEntityId == accountLegalEntityId
                                      select new { application, account, apprentice };
            
            var dtoList = (from accountApplication in accountApplications
                           let dto = new ApprenticeApplicationDto
                           {
                               AccountId = accountApplication.application.AccountId,
                               ApplicationDate = accountApplication.application.DateCreated,
                               ApplicationId = accountApplication.application.Id,
                               FirstName = accountApplication.apprentice.FirstName,
                               LastName = accountApplication.apprentice.LastName,
                               LegalEntityName = accountApplication.account.LegalEntityName,
                               Status = accountApplication.application.Status.ToString(),
                               TotalIncentiveAmount = accountApplication.apprentice.TotalIncentiveAmount
                           }
                           select dto).ToList();
            return await Task.FromResult(dtoList);
        }
    }
}
