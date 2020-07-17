using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.NewApprenticeIncentive
{
    public class NewApprenticeIncentive : AggregateRoot
    {
        private static readonly DateTime EligibilityStartDate = new DateTime(2020, 8, 1);

        public static bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.StartDate < EligibilityStartDate || !apprenticeship.IsApproved)
            {
                return false;
            }

            return true;
        }
    }
}
