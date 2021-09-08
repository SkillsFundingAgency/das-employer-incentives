﻿using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase1Incentive : Incentive
    {
        public Phase1Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate) : base(dateOfBirth, startDate, incentiveType, breaksInLearning, submissionDate)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;
        protected override int DelayPeriod => 0;
        public override List<PaymentProfile> PaymentProfiles =>
            new List<PaymentProfile>
            {
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1000),
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1000),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 750),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 750)
            };

        protected override DateTime CalculateDueDate(PaymentProfile paymentProfile, DateTime submissionDate)
        {
            return StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
        }

        private static List<EligibilityPeriod> EligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibilityPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = EligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? EligibilityPeriods.First().MinimumAgreementVersion;
        }
    }
}
