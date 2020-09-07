using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        private readonly DateTime _startDate;
        private readonly List<IncentivePaymentProfile> _incentivePaymentProfiles;
        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 1, 31);
        public List<PendingPayment> PendingPayments;

        public Incentive(DateTime dateOfBirth, DateTime startDate,
            List<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _incentivePaymentProfiles = incentivePaymentProfiles;
            PendingPayments = GenerateEarningsForApprenticeship();
        }

        public bool IsApprenticeshipEligible => _startDate >= EligibilityStartDate && _startDate <= EligibilityEndDate;

        public decimal TotalIncentiveAmount => PendingPayments.Sum(x => x.AmountPayable);

        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

        private int AgeAtStartOfCourse()
        {
            var age = _startDate.Year - _dateOfBirth.Year;
            if (_startDate.DayOfYear < _dateOfBirth.DayOfYear)
            {
                age--;
            }

            return age;
        }

        private List<PendingPayment> GenerateEarningsForApprenticeship()
        {
            var earnings = new List<PendingPayment>();

            if (!IsApprenticeshipEligible)
            {
                return earnings;
            }

            if (_incentivePaymentProfiles == null)
            {
                return earnings;
            }

            var incentivePaymentProfile = _incentivePaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType);

            if (incentivePaymentProfile?.PaymentProfiles == null)
            {
                return earnings;
            }

            foreach (var paymentProfile in incentivePaymentProfile.PaymentProfiles)
            {
                earnings.Add(new PendingPayment
                {
                    DatePayable = _startDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart),
                    AmountPayable = paymentProfile.AmountPayable
                });
            }

            return earnings;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return _startDate;

            foreach (var earning in PendingPayments)
            {
                yield return earning.AmountPayable;
                yield return earning.DatePayable;
            }
        }
    }

    public class PendingPayment
    {
        public DateTime DatePayable { get; set; }
        public decimal AmountPayable { get; set; }
    }
}