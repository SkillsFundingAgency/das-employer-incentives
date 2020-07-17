using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.NewApprenticeIncentive
{
    public class NewApprenticeIncentive : AggregateRoot
    {
        private static DateTime _eligibilityStartDate = new DateTime(2020, 8, 1);

        public static bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.StartDate < _eligibilityStartDate || !apprenticeship.IsApproved)
            {
                return false;
            }

            return true;
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
