using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
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

        public async Task<Learner> GetByApprenticeshipIncentiveId(Guid apprenticeshipIncentiveId)
        {
            var learner = await _dbContext.Learners
                            .FirstOrDefaultAsync(a => a.ApprenticeshipIncentiveId == apprenticeshipIncentiveId);

            if (learner == null)
            {
                return null;
            }

            var nextPayment = await _dbContext.PendingPayments.Where(pp => pp.ApprenticeshipIncentiveId == apprenticeshipIncentiveId
                && pp.PaymentMadeDate == null)
                .OrderBy(pp => pp.DueDate).FirstOrDefaultAsync();

            return learner.Map(nextPayment);
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
