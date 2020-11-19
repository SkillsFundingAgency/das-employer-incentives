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
        private readonly List<Payment> _payments;
        private readonly IEnumerable<IncentivePaymentProfile> _incentivePaymentProfiles;

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 1, 31);
        
        public IEnumerable<Payment> Payments => _payments;
        public bool IsEligible => _startDate >= EligibilityStartDate && _startDate <= EligibilityEndDate;
        public decimal Total => Payments.Sum(x => x.Amount);
        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

        public Incentive(DateTime dateOfBirth, DateTime startDate, IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _incentivePaymentProfiles = incentivePaymentProfiles;
            _payments = GeneratePayments();
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

            foreach (var paymentProfile in incentivePaymentProfile.PaymentProfiles)
            {
                payments.Add(new Payment(paymentProfile.AmountPayable, _startDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart)));
            }

            return payments;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return _startDate;

            foreach (var payment in Payments)
            {
                yield return payment.Amount;
                yield return payment.PaymentDate;
            }
        }
    }
}