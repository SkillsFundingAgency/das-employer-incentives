using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        private readonly DateTime _startDate;
        private readonly int _breakInLearningDayCount;
        private readonly List<Payment> _payments;
        private readonly IEnumerable<IncentivePaymentProfile> _incentivePaymentProfiles;
        private readonly List<EarningType> _earningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public static DateTime LatestCommitmentStartDate = new DateTime(2021, 3, 31);

        private readonly List<EligibiliyPeriod> EligibilityPeriods = new List<EligibiliyPeriod>
        {
            new EligibiliyPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibiliyPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public IEnumerable<Payment> Payments => _payments;
        public bool IsEligible => _startDate >= EligibilityStartDate && _startDate <= EligibilityEndDate;
        public decimal Total => Payments.Sum(x => x.Amount);
        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

        public Incentive(
            DateTime dateOfBirth, 
            DateTime startDate, 
            IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles,
            int breakInLearningDayCount)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _incentivePaymentProfiles = incentivePaymentProfiles;
            _breakInLearningDayCount = breakInLearningDayCount;
            _payments = GeneratePayments();
        }

        public bool IsNewAgreementRequired(int signedagreementVersion)
        {
            if (!IsEligible)
            {
                return true;
            }

            var applicablePeriod = EligibilityPeriods.Single(x => x.StartDate <= _startDate && x.EndDate >= _startDate);
            return signedagreementVersion < applicablePeriod.MinimumAgreementVersion;
        }

        private int AgeAtStartOfCourse()
        {
            return _dateOfBirth.AgeOnThisDay(_startDate);
        }

        private List<Payment> GeneratePayments()
        {
            var payments = new List<Payment>();

            if (!IsEligible)
            {
                return payments;
            }

            var incentivePaymentProfile = _incentivePaymentProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType);

            if (incentivePaymentProfile?.PaymentProfiles == null)
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {IncentiveType}");
            }

            var paymentIndex = 0;
            foreach (var paymentProfile in incentivePaymentProfile.PaymentProfiles)
            {
                payments.Add(new Payment(paymentProfile.AmountPayable, _startDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart).AddDays(_breakInLearningDayCount), _earningTypes[paymentIndex]));
                paymentIndex++;
            }

            return payments;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return _startDate;
            yield return _breakInLearningDayCount;

            foreach (var payment in Payments)
            {                
                yield return payment.Amount;
                yield return payment.PaymentDate;
                yield return payment.EarningType;
            }
        }

        private class EligibiliyPeriod
        {
            public DateTime StartDate { get; }
            public DateTime EndDate { get; }
            public int MinimumAgreementVersion { get; }

            public EligibiliyPeriod(DateTime startDate, DateTime endDate, int minimumAgreementVersion)
            {
                StartDate = startDate;
                EndDate = endDate;
                MinimumAgreementVersion = minimumAgreementVersion;
            }
        }
    }
}