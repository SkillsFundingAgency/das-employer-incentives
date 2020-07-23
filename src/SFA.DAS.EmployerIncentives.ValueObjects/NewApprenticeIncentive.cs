using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class NewApprenticeIncentive : ValueObject
    {
        private readonly DateTime EligibilityStartDate = new DateTime(2020, 8, 1);

        public bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.StartDate < EligibilityStartDate || !apprenticeship.IsApproved)
            {
                return false;
            }

            return true;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return EligibilityStartDate;
        }
    }
}
