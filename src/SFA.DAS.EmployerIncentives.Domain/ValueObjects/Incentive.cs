using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        private readonly DateTime _startDate;
        private readonly IncentiveProfiles _profiles;
        public IEnumerable<Payment> Payments { get; }
        public bool IsEligible => _profiles.IsEligible(_startDate);
        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

        public Incentive(DateTime dateOfBirth, DateTime startDate, IncentiveProfiles profiles)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _profiles = profiles;
            Payments = GeneratePayments();
        }

        public bool IsNewAgreementRequired(int signedAgreementVersion)
        {
            if (!IsEligible) return true;

            var minimumRequiredAgreementVersion = _profiles.GetMinimumAgreementVersion(_startDate);
            return signedAgreementVersion < minimumRequiredAgreementVersion;
        }

        private int AgeAtStartOfCourse()
        {
            return _dateOfBirth.AgeOnThisDay(_startDate);
        }

        private IEnumerable<Payment> GeneratePayments()
        {
            if (!IsEligible) return new List<Payment>();

            var paymentProfiles = _profiles.GetPaymentProfiles(IncentiveType, _startDate);

            return paymentProfiles.Select(profile =>
                new Payment(
                    profile.AmountPayable, 
                    _startDate.AddDays(profile.DaysAfterApprenticeshipStart),
                    profile.EarningType)
            );
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return _startDate;

            foreach (var payment in Payments)
            {                
                yield return payment.Amount;
                yield return payment.PaymentDate;
                yield return payment.EarningType;
            }
        }
    }

}