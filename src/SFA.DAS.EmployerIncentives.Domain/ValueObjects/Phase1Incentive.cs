using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Phase1Incentive : Incentive
    {
        public Phase1Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning) : base(dateOfBirth, startDate, paymentProfiles, breaksInLearning)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

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
