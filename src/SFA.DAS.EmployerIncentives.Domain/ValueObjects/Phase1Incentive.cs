using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

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

        public override List<EligibilityPeriod> EligibilityPeriods => _eligibilityPeriods;
        private static List<EligibilityPeriod> _eligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibilityPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = _eligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? _eligibilityPeriods.First().MinimumAgreementVersion;
        }
        public new static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase != Phase.Phase1)
            {
                throw new InvalidPhaseException();
            }
            return true;
        }
    }
}
