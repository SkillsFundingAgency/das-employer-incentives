using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class PhaseNotSetIncentive : Incentive
    {
        public PhaseNotSetIncentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount,
            DateTime submissionDate) : base(dateOfBirth, startDate, incentiveType, breakInLearningDayCount, submissionDate)
        {
        }

        public override bool IsEligible => false;

        public override List<PaymentProfile> PaymentProfiles => new List<PaymentProfile>();

        public override List<EligibilityPeriod> EligibilityPeriods => new List<EligibilityPeriod>();

        protected override int? DelayPeriod => null;

        protected override DateTime CalculateMinimumDueDate(PaymentProfile paymentProfile, DateTime submissionDate)
        {
            return DateTime.MinValue;
        }
    }
}
