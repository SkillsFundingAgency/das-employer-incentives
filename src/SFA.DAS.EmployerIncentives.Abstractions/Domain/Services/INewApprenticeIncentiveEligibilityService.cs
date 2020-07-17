using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Abstractions.Domain.Services
{
    public interface INewApprenticeIncentiveEligibilityService
    {
        bool IsApprenticeshipEligible(Apprenticeship apprenticeship);
    }
}
