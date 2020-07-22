using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Services
{
    public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        public bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            var incentive = new NewApprenticeIncentive();
            return incentive.IsApprenticeshipEligible(apprenticeship);
        }
    }
}
