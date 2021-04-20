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
        private readonly IncentivesConfiguration _configuration;
        private readonly List<EarningType> _earningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);

        //private readonly List<EligibiliyPeriod> EligibilityPeriods = new List<EligibiliyPeriod>
        //{
        //    new EligibiliyPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
        //    new EligibiliyPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 3, 31), 5)
        //};

        public IEnumerable<Payment> Payments { get; }
        public bool IsEligible { get; }

        public decimal Total => Payments.Sum(x => x.Amount);
        public IncentiveType IncentiveType => AgeAtStartOfCourse() >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

        public Incentive(DateTime dateOfBirth, DateTime startDate, IncentivesConfiguration configuration)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _configuration = configuration;
            Payments = GeneratePayments();
            IsEligible = configuration.IsEligible(_startDate);
        }

        public bool IsNewAgreementRequired(int signedAgreementVersion)
        {
            if (!IsEligible)
            {
                return true;
            }

            byte minimumRequiredAgreementVersion = _configuration.GetMinimumAgreementVersion(_startDate);
            // var applicablePeriod = EligibilityPeriods.Single(x => x.StartDate <= _startDate && x.EndDate >= _startDate);
            return signedAgreementVersion < minimumRequiredAgreementVersion;
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

           ////  var incentivePaymentProfile = _incentiveProfiles.FirstOrDefault(x => x.IncentiveType == IncentiveType);
            var profiles = _configuration.GetPaymentProfiles(IncentiveType, _startDate);

            payments.AddRange(profiles.Select((t, i) => 
                new Payment(t.AmountPayable, _startDate.AddDays(t.DaysAfterApprenticeshipStart), _earningTypes[i])));

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
                yield return payment.EarningType;
            }
        }

        //private class EligibiliyPeriod
        //{
        //    public DateTime StartDate { get; }
        //    public DateTime EndDate { get; }
        //    public int MinimumAgreementVersion { get; }

        //    public EligibiliyPeriod(DateTime startDate, DateTime endDate, int minimumAgreementVersion)
        //    {
        //        StartDate = startDate;
        //        EndDate = endDate;
        //        MinimumAgreementVersion = minimumAgreementVersion;
        //    }
        //}
    }
}