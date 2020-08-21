using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IUlnValidationService
    {
        Task<bool> UlnAlreadyOnSubmittedIncentiveApplication(long uln);
    }
}
