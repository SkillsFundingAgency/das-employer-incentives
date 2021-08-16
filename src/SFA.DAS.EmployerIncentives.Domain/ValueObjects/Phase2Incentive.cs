using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase2Incentive : Incentive
    {
        public Phase2Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount,
            IDateTimeService dateTimeService) : base(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount, dateTimeService)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2021, 4, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 11, 30);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

        public static int MinimumAgreementVersion() => 6;
    }
}
