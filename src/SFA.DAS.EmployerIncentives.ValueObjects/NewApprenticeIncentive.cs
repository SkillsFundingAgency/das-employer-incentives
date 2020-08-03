using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class NewApprenticeIncentive : ValueObject
    {
        private readonly DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        private const decimal TwentyFiveOrOverIncentiveAmount = 2000;
        private const decimal UnderTwentyFiveIncentiveAmount = 1500;

        public bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.StartDate < EligibilityStartDate || !apprenticeship.IsApproved)
            {
                return false;
            }

            return true;
        }

        public decimal CalculateTotalIncentiveAmount(DateTime apprenticeDateOfBirth, DateTime plannedStartDate)
        {
            var apprenticeAge = CalculateAgeAtStartOfApprenticeship(apprenticeDateOfBirth, plannedStartDate);

            if (apprenticeAge > 24)
            {
                return UnderTwentyFiveIncentiveAmount;
            }

            return TwentyFiveOrOverIncentiveAmount;
        }

        private int CalculateAgeAtStartOfApprenticeship(in DateTime apprenticeDateOfBirth, in DateTime plannedStartDate)
        {
            var age = plannedStartDate.Year - apprenticeDateOfBirth.Year;

            if (apprenticeDateOfBirth.Date > plannedStartDate.AddYears(-age)) age--;

            return age;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return EligibilityStartDate;
        }
    }
}
