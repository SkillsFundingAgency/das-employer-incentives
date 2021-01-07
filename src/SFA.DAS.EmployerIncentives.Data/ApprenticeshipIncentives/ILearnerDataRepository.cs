using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface ILearnerDataRepository
    {
        Task<LearnerModel> GetByApprenticeshipIncentiveId(Guid incentiveId);
        Task<LearnerModel> Get(Guid id);
        Task Add(LearnerModel learnerModel);
        Task Update(LearnerModel learnerModel);
    }
}
