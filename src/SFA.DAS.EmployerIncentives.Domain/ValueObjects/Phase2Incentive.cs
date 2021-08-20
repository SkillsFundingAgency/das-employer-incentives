using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase2Incentive : Incentive
    {
        public Phase2Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount,
            DateTime submissionDate) : base(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount, submissionDate)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2021, 4, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 11, 30);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;
        protected override int DelayPeriod => 21;
        protected override DateTime CalculateDueDate(PaymentProfile paymentProfile, DateTime submissionDate)
        {
            var minimumDueDate = submissionDate.AddDays(DelayPeriod);

            var paymentDueDate = StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
            if (paymentDueDate < minimumDueDate)
            {
                paymentDueDate = minimumDueDate;
            }

            return paymentDueDate;
        }

        public static int MinimumAgreementVersion() => 6;
    }
}
