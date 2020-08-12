using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IUlnQueryRepository
    {
        Task<bool> UlnAlreadyOnSubmittedIncentiveApplication(long uln);
    }
}
