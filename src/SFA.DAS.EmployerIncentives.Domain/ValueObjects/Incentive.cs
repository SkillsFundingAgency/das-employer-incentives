using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        private readonly DateTime _startDate;
        private readonly IEnumerable<IncentivePaymentProfile> _incentivePaymentProfiles;
        public IEnumerable<Payment> Payments { get; }
        public bool IsEligible => IncentivePaymentProfile != null;
        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;
      
        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);

        public Incentive(DateTime dateOfBirth, DateTime startDate, IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _incentivePaymentProfiles = incentivePaymentProfiles;
            Payments = GeneratePayments();
        }

        public bool IsNewAgreementRequired(int signedAgreementVersion) => !IsEligible || signedAgreementVersion < IncentivePaymentProfile.MinRequiredAgreementVersion;
    
        private IncentivePaymentProfile IncentivePaymentProfile => _incentivePaymentProfiles.SingleOrDefault(x =>
            x.EligibleTrainingDates.Start <= _startDate && x.EligibleTrainingDates.End >= _startDate &&
            x.EligibleApplicationDates.Start <= _startDate && x.EligibleApplicationDates.End >= _startDate);


        private int AgeAtStartOfCourse()
        {
            return _dateOfBirth.AgeOnThisDay(_startDate);
        }

        private IEnumerable<Payment> GeneratePayments()
        {
            if (!IsEligible) return Enumerable.Empty<Payment>();

            var paymentProfiles = IncentivePaymentProfile.PaymentProfiles.Where(x => x.IncentiveType == IncentiveType).ToList();

            if (!paymentProfiles.Any())
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {IncentiveType} with Start Date {_startDate}");
            }

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