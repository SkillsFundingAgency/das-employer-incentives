using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class LearnerDataRepository : ILearnerDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;

        public LearnerDataRepository(EmployerIncentivesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Learner> Get(ApprenticeshipIncentive incentive)
        {
            var existing = await _dbContext.Learners
                .FirstOrDefaultAsync(a => a.ApprenticeshipIncentiveId == incentive.Id);

            Learner learner;
            if (existing != null)
            {
                learner = existing.Map();
            }
            else
            {
                learner = new Learner(
                    Guid.NewGuid(),
                    incentive.Id,
                    incentive.Apprenticeship.Id,
                    incentive.Apprenticeship.Provider.Ukprn,
                    incentive.Apprenticeship.UniqueLearnerNumber,
                    DateTime.UtcNow
                );
            }

            var nextPayment = incentive.PendingPayments.Where(pp => pp.PaymentMadeDate == null)
                .OrderBy(pp => pp.DueDate).FirstOrDefault();

            if (nextPayment?.PaymentYear != null && nextPayment.PeriodNumber.HasValue)
            {
                learner.SetNextPendingPayment(new NextPendingPayment(
                    nextPayment.PaymentYear.Value, nextPayment.PeriodNumber.Value, nextPayment.DueDate));
            }

            return learner;
        }

        public async Task Save(Learner learner)
        {
            var updatedLearner = learner.Map();

            var existingLearner = await _dbContext.Learners.FirstOrDefaultAsync(x => x.Id == updatedLearner.Id);

            if (existingLearner != null)
            {
                _dbContext.Entry(existingLearner).CurrentValues.SetValues(updatedLearner);
            }
            else
            {
                _dbContext.Learners.Add(updatedLearner);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
