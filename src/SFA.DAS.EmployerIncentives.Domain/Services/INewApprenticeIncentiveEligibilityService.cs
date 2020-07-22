using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public interface INewApprenticeIncentiveEligibilityService
    {
        bool IsApprenticeshipEligible(Apprenticeship apprenticeship);
    }
}
