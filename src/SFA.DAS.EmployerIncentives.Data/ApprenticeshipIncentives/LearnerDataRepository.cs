using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class LearnerDataRepository : ILearnerDataRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public LearnerDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(LearnerModel learnerModel)
        {
            await _dbContext.AddAsync(learnerModel.Map());
        }

        public async Task<LearnerModel> Get(Guid id)
        {
            var learner = await _dbContext.Learners
                           .FirstOrDefaultAsync(a => a.Id == id);

            if (learner == null)
            {
                return null;
            }

            return learner.Map();
        }

        public async Task<LearnerModel> GetByApprenticeshipIncentiveId(Guid incentiveId)
        {
            var learner = await _dbContext.Learners
                            .FirstOrDefaultAsync(a => a.ApprenticeshipIncentiveId == incentiveId);

            if (learner == null)
            {
                return null;
            }

            return learner.Map();
        }

        public async Task Update(LearnerModel learnerModel)
        {
            var updatedLearner = learnerModel.Map();

            var existingLearner = await _dbContext.Learners.FirstOrDefaultAsync(x => x.Id == updatedLearner.Id);

            if (existingLearner != null)
            {
                _dbContext.Entry(existingLearner).CurrentValues.SetValues(updatedLearner);
            }
        }
    }
}
