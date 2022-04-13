using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner
{
    public interface ILearnerService
    {
        Task<Learner> Refresh(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive incentive);
    }
}
