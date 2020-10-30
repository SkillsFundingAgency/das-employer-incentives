using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerIncentives.Data.EarningsResilienceCheck
{
    public class EarningsResilienceCheckRepository : IEarningsResilienceCheckRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public EarningsResilienceCheckRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<EarningsResilienceCheckDto>> GetApplicationsWithoutEarningsCalculations()
        {
            var earningsCheckApplications = new List<EarningsResilienceCheckDto>();

            var applicationsWithoutApprenticeshipIncentives = await (from applications in _dbContext.Applications
                                                              join apprenticeships in _dbContext.ApplicationApprenticeships
                                                              on applications.Id equals apprenticeships.IncentiveApplicationId                 
                                                              where apprenticeships.ApprenticeshipIncentives.Count() == 0
                                                              select new { applications.AccountId, applications.Id }).ToListAsync();
                       
            var applicationsWithoutPendingPayments = await (from applications in _dbContext.Applications
                                                     join apprenticeships in _dbContext.ApplicationApprenticeships
                                                     on applications.Id equals apprenticeships.IncentiveApplicationId
                                                     join incentives in _dbContext.ApprenticeshipIncentives
                                                     on apprenticeships.ApprenticeshipId equals incentives.ApprenticeshipId
                                                     where incentives.PendingPayments.Count() == 0
                                                     select new { applications.AccountId, applications.Id }).ToListAsync();

            earningsCheckApplications.AddRange(from application in applicationsWithoutApprenticeshipIncentives
                                               select new EarningsResilienceCheckDto { AccountId = application.AccountId, ApplicationId = application.Id });

            earningsCheckApplications.AddRange(from application in applicationsWithoutPendingPayments
                                               select new EarningsResilienceCheckDto { AccountId = application.AccountId, ApplicationId = application.Id });

            return await Task.FromResult(earningsCheckApplications);
        }
    }
}
