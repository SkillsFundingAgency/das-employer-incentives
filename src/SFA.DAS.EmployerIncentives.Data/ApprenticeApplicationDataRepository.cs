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

        public async Task<List<ApprenticeApplicationDto>> GetList(long accountId)
        {
            var accountApplications = from application in _dbContext.Applications
                                      join account in _dbContext.Accounts
                                      on application.AccountLegalEntityId equals account.AccountLegalEntityId
                                      join apprentice in _dbContext.ApplicationApprenticeships
                                      on application.Id equals apprentice.IncentiveApplicationId
                                      where application.AccountId == accountId
                                      select new { application, account, apprentice };
            
            var dtoList = new List<ApprenticeApplicationDto>();
            foreach (var accountApplication in accountApplications)
            {
                var dto = new ApprenticeApplicationDto
                    {
                        AccountId = accountApplication.application.AccountId,
                        ApplicationDate = accountApplication.application.DateCreated,
                        ApplicationId = accountApplication.application.Id,
                        FirstName = accountApplication.apprentice.FirstName,
                        LastName = accountApplication.apprentice.LastName,
                        LegalEntityName = accountApplication.account.LegalEntityName,
                        Status = accountApplication.application.Status.ToString(),
                        TotalIncentiveAmount = accountApplication.apprentice.TotalIncentiveAmount
                    };
                dtoList.Add(dto);
            }

            return await Task.FromResult(dtoList);
        }
    }
}
