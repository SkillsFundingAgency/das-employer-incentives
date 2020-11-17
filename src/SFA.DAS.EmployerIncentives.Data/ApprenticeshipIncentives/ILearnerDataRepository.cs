using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface ILearnerDataRepository
    {
        Task Save(Learner learner);
        Task<Learner> Get(ApprenticeshipIncentive incentive);
    }
}
