﻿using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase3Incentive : Incentive
    {
        public Phase3Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount,
            DateTime submissionDate) : base(dateOfBirth, startDate, incentiveType, breakInLearningDayCount, submissionDate)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2021, 10, 01);
        public static DateTime EligibilityEndDate = new DateTime(2022, 01, 31);
        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 10, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2022, 03, 31);
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
        
        private static List<EligibilityPeriod> _eligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2021, 10, 1), new DateTime(2022, 1, 31), 7)
        };
        public override List<EligibilityPeriod> EligibilityPeriods => _eligibilityPeriods;

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = _eligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? _eligibilityPeriods.First().MinimumAgreementVersion;
        }

        public new static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase != Phase.Phase3)
            {
                throw new InvalidPhaseException();
            }

            if (apprenticeship.EmploymentStartDate.HasValue &&
                (apprenticeship.EmploymentStartDate.Value.Date >= EmployerEligibilityStartDate.Date) &&
                (apprenticeship.EmploymentStartDate.Value.Date <= EmployerEligibilityEndDate.Date))
            {
                return true;
            }

            return false;
        }
    }
}