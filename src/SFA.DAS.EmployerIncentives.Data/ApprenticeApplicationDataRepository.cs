using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class ApprenticeApplicationDataRepository : IApprenticeApplicationDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public ApprenticeApplicationDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _dbContext = dbContext.Value;
        }

        public async Task<List<ApprenticeApplicationDto>> GetList(long accountId, long accountLegalEntityId)
        {
            var accountApplications = from incentive in _dbContext.ApprenticeshipIncentives
                                      from account in _dbContext.Accounts.Where(x => x.AccountLegalEntityId == incentive.AccountLegalEntityId)
                                      from firstPayment in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.FirstPayment).DefaultIfEmpty()
                                      from secondPayment in _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == incentive.Id && x.EarningType == EarningType.SecondPayment).DefaultIfEmpty()
                                      from learner in _dbContext.Learners.Where(x => x.ApprenticeshipIncentiveId == incentive.Id).DefaultIfEmpty()
                                      where incentive.AccountId == accountId && incentive.AccountLegalEntityId == accountLegalEntityId
                                      select new { incentive, account, firstPayment, secondPayment, learner };

            return await (from data in accountApplications
                          let dto = new ApprenticeApplicationDto
                          {
                              AccountId = data.incentive.AccountId,
                              AccountLegalEntityId = data.incentive.AccountLegalEntityId,
                              ApplicationDate = data.incentive.SubmittedDate ?? DateTime.Now,
                              FirstName = data.incentive.FirstName,
                              LastName = data.incentive.LastName,
                              ULN = data.incentive.ULN,
                              LegalEntityName = data.account.LegalEntityName,
                              SubmittedByEmail = data.incentive.SubmittedByEmail,
                              TotalIncentiveAmount = data.incentive.PendingPayments.Sum(x => x.Amount),
                              CourseName = data.incentive.CourseName,
                              FirstPaymentStatus = data.firstPayment == default ? null : new PaymentStatusDto
                              {
                                  PaymentDate = data.firstPayment.DueDate.AddMonths(1),
                                  LearnerMatchNotFound = (!data.learner.LearningFound.HasValue || !data.learner.LearningFound.Value),
                                  PaymentAmount = data.firstPayment.Amount,
                                  HasDataLock = (data.learner.HasDataLock.HasValue && data.learner.HasDataLock.Value),
                                  ApprenticeNotInLearning = (data.learner.InLearning.HasValue && !data.learner.InLearning.Value)
                              },
                              SecondPaymentStatus = data.secondPayment == default ? null : new PaymentStatusDto
                              {
                                  PaymentDate = data.secondPayment.DueDate.AddMonths(1),
                                  LearnerMatchNotFound = (!data.learner.LearningFound.HasValue || !data.learner.LearningFound.Value),
                                  PaymentAmount = data.secondPayment.Amount,
                                  HasDataLock = (data.learner.HasDataLock.HasValue && data.learner.HasDataLock.Value)
                              }
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
