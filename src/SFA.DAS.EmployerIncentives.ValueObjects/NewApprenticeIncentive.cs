using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.ValueObjects
{
    public class NewApprenticeIncentive : ValueObject
    {
        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 1, 31);
        private const decimal TwentyFiveOrOverIncentiveAmount = 2000;
        private const decimal UnderTwentyFiveIncentiveAmount = 1500;

        public bool IsApprenticeshipEligible(Apprenticeship apprenticeship)
        {
            if (IsStartDateWithinSchemeRange(apprenticeship) || !apprenticeship.IsApproved)
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

        private bool IsStartDateWithinSchemeRange(Apprenticeship apprenticeship)
        {
            return apprenticeship.StartDate < EligibilityStartDate || apprenticeship.StartDate > EligibilityEndDate;
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
            yield return EligibilityEndDate;
        }
    }
}
