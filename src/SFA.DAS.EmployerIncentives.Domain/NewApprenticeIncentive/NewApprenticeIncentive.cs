using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.NewApprenticeIncentive
{
    public class NewApprenticeIncentive : AggregateRoot
    {
        public static bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            return false;
        }
    }

    /* public class NewApprenticeIncentiveEligibilityService : INewApprenticeIncentiveEligibilityService
    {
        public IEnumerable<Apprenticeship.Apprenticeship> GetEligibileApprenticeships(NewApprenticeIncentive incentive, IEnumerable<Apprenticeship.Apprenticeship> apprenticeships)
        {
            return apprenticeships.Where(x => incentive.IsApprenticeshipEligible(x));
        }
    } */
}
