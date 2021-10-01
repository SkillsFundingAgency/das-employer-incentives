using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi
{
    public interface ILearnerService
    {
        Task<LearnerServiceResponse> Get(Learner learner);
    }
}
