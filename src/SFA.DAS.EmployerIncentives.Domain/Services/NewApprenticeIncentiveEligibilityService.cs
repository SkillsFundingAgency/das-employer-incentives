using SFA.DAS.EmployerIncentives.Abstractions.Domain.Services;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        public bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            return NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);
        }
    }
}
