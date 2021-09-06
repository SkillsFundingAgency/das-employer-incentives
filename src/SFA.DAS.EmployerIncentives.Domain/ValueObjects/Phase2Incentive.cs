using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase2Incentive : Incentive
    {
        public Phase2Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount,
            DateTime submissionDate) : base(dateOfBirth, startDate, incentiveType, breakInLearningDayCount, submissionDate)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2021, 4, 1);
        public static DateTime EligibilityEndDate = new DateTime(2022, 01, 31);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;
        protected override int DelayPeriod => 21;
        public override List<PaymentProfile> PaymentProfiles =>
            new List<PaymentProfile>
            {
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500)
            };

        protected override DateTime CalculateDueDate(PaymentProfile paymentProfile, DateTime submissionDate)
        {
            var minimumDueDate = submissionDate.Date.AddDays(DelayPeriod);

            var paymentDueDate = StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
            if (paymentDueDate < minimumDueDate)
            {
                paymentDueDate = minimumDueDate;
            }

            return paymentDueDate;
        }

        public static int MinimumAgreementVersion() => 6;

        private static List<EligibilityPeriod> EligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2021, 4, 1), new DateTime(2021, 9, 30), 6),
            new EligibilityPeriod(new DateTime(2021, 10, 1), new DateTime(2022, 1, 31), 7)
        };

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = EligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? EligibilityPeriods.First().MinimumAgreementVersion;
        }
    }
}
