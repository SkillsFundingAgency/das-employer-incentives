using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
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
            var learner = learnerModel.Map();
            learner.CreatedDate = DateTime.Now;
            learner.LearningPeriods.ToList().ForEach(l => l.CreatedDate = learner.CreatedDate);
            learner.DaysInLearnings.ToList().ForEach(l => l.CreatedDate = learner.CreatedDate);
            await _dbContext.AddAsync(learner);
        }

        public async Task<LearnerModel> Get(Guid id)
        {
            var learner = await _dbContext.Learners
                .Include(x => x.LearningPeriods)
                .Include(x => x.DaysInLearnings)
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
                .Include(x => x.LearningPeriods)
                .Include(x => x.DaysInLearnings)
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
                UpdateLearner(updatedLearner, existingLearner);
            }
        }

        private void UpdateLearner(Learner updatedLearner, Learner existingLearner)
        {
            updatedLearner.CreatedDate = existingLearner.CreatedDate;
            updatedLearner.UpdatedDate = DateTime.Now;
            _dbContext.Entry(existingLearner).CurrentValues.SetValues(updatedLearner);

            RemoveDeletedLearningPeriods(updatedLearner, existingLearner);
            RemoveDeletedDaysinLearnings(updatedLearner, existingLearner);

            foreach (var learningPeriod in updatedLearner.LearningPeriods)
            {                
                var existingLearningPeriod = existingLearner
                    .LearningPeriods
                    .SingleOrDefault(p => p.LearnerId == learningPeriod.LearnerId && 
                    p.StartDate == learningPeriod.StartDate);

                if (existingLearningPeriod != null)
                {
                    learningPeriod.CreatedDate = existingLearningPeriod.CreatedDate;
                    _dbContext.Entry(existingLearningPeriod).CurrentValues.SetValues(learningPeriod);                   
                }
                else
                {
                    learningPeriod.CreatedDate = DateTime.Now;
                    _dbContext.LearningPeriods.Add(learningPeriod);
                }
            }

            foreach (var daysInLearning in updatedLearner.DaysInLearnings)
            {
                var existingDaysInLearning = existingLearner
                    .DaysInLearnings
                    .SingleOrDefault(p => p.LearnerId == daysInLearning.LearnerId && 
                    p.CollectionPeriodNumber == daysInLearning.CollectionPeriodNumber && 
                    p.CollectionPeriodYear == daysInLearning.CollectionPeriodYear);

                if (existingDaysInLearning != null)
                {
                    daysInLearning.CreatedDate = existingDaysInLearning.CreatedDate;
                    
                    _dbContext.Entry(existingDaysInLearning).CurrentValues.SetValues(daysInLearning);
                    if (_dbContext.Entry(existingDaysInLearning).State == EntityState.Modified)
                    {
                        existingDaysInLearning.UpdatedDate = DateTime.Now;
                    }
                }
                else
                {
                    daysInLearning.CreatedDate = DateTime.Now;
                    _dbContext.DaysInLearnings.Add(daysInLearning);
                }
            }
        }

        private void RemoveDeletedLearningPeriods(Learner updatedLearner, Learner existingLearner)
        {
            foreach (var existingLearningPeriod in existingLearner.LearningPeriods)
            {
                if (!updatedLearner
                    .LearningPeriods
                    .Any(p => p.LearnerId == existingLearningPeriod.LearnerId &&
                        p.StartDate == existingLearningPeriod.StartDate))
                {
                    _dbContext.LearningPeriods.Remove(existingLearningPeriod);
                }
            }
        }

        private void RemoveDeletedDaysinLearnings(Learner updatedLearner, Learner existingLearner)
        {
            foreach (var existingDaysinLearning in existingLearner.DaysInLearnings)
            {
                if (!updatedLearner
                    .DaysInLearnings
                    .Any(d => d.LearnerId == existingDaysinLearning.LearnerId &&
                        d.CollectionPeriodNumber == existingDaysinLearning.CollectionPeriodNumber && 
                        d.CollectionPeriodYear == existingDaysinLearning.CollectionPeriodYear))
                {
                    _dbContext.DaysInLearnings.Remove(existingDaysinLearning);
                }
            }
        }
    }
}
