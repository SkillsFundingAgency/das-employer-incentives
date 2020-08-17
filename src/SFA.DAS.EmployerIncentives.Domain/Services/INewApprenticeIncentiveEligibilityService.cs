using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public interface INewApprenticeIncentiveEligibilityService
    {
        Task<bool> IsApprenticeshipEligible(Apprenticeship apprenticeship);
    }
}
