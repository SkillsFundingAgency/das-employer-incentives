using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface ILearnerDataRepository
    {
        Task<Learner> GetByApprenticeshipIncentiveId(Guid apprenticeshipIncentiveId);
        Task Save(Learner learner);
    }
}
