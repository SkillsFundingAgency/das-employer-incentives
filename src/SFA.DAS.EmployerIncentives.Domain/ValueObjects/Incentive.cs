using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public abstract class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        protected readonly DateTime StartDate;
        private readonly IEnumerable<PaymentProfile> _paymentProfiles;
        private readonly IReadOnlyCollection<BreakInLearning> _breaksInLearning;
        private readonly DateTime _submissionDate;
        public static readonly List<EarningType> EarningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };
        public IReadOnlyCollection<Payment> Payments => Generate(_paymentProfiles, _breaksInLearning, _submissionDate).AsReadOnly();
        public abstract bool IsEligible { get; }

        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 04, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2021, 09, 30);

        protected abstract int DelayPeriod { get; }

        protected abstract DateTime CalculateMinimumDueDate(PaymentProfile paymentProfile, DateTime submissionDate);

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _paymentProfiles = paymentProfiles;
            _breaksInLearning = breaksInLearning;
            _submissionDate = submissionDate;
        }

        public static Incentive Create(
            ApprenticeshipIncentive incentive,
            IEnumerable<IncentivePaymentProfile> paymentProfiles)
        {
            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, paymentProfiles, incentive.BreakInLearnings, incentive.SubmissionDate);
        }

        public static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.EmploymentStartDate.HasValue &&
                (apprenticeship.EmploymentStartDate.Value.Date >= EmployerEligibilityStartDate.Date) &&
                (apprenticeship.EmploymentStartDate.Value.Date <= EmployerEligibilityEndDate.Date))
            {
                return true;
            }

            return false;
        }

        public int MinimumDaysInLearning(EarningType earningType)
        {
            var paymentProfile = _paymentProfiles.ElementAt(EarningTypes.IndexOf(earningType));
            return paymentProfile.DaysAfterApprenticeshipStart;
        }

        private static List<PaymentProfile> GetPaymentProfiles(Phase phase, DateTime dateOfBirth, DateTime startDate, IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles)
        {
            var incentivePaymentProfile = incentivePaymentProfiles.FirstOrDefault(x => x.IncentivePhase.Identifier == phase);

            if (incentivePaymentProfile?.PaymentProfiles == null)
            {
                throw new MissingPaymentProfileException($"Incentive Payment profile not found for IncentivePhase {phase}");
            }

            var incentiveType = AgeAtStartOfCourse(dateOfBirth, startDate) >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

            var paymentProfiles = incentivePaymentProfile.PaymentProfiles.Where(x => x.IncentiveType == incentiveType).ToList();

            if (!paymentProfiles.Any())
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {incentiveType}");
            }

            return paymentProfiles;
        }

        private static int AgeAtStartOfCourse(DateTime dateOfBirth, DateTime startDate)
        {
            return dateOfBirth.AgeOnThisDay(startDate);
        }

        protected List<Payment> Generate(IEnumerable<PaymentProfile> paymentProfiles, IReadOnlyCollection<BreakInLearning> breaksInLearning, DateTime submissionDate)
        {
            var payments = new List<Payment>();
            if (!IsEligible) return payments;

            var paymentIndex = 0;
            foreach (var paymentProfile in paymentProfiles)
            {
                var paymentDueDate = StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
                var minimumDueDate = CalculateMinimumDueDate(paymentProfile, submissionDate);
                payments.Add(new Payment(paymentProfile.AmountPayable, paymentDueDate, EarningTypes[paymentIndex], breaksInLearning, minimumDueDate));
                paymentIndex++;
            }

            return payments;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return StartDate;

            foreach (var payment in Payments)
            {                
                yield return payment.Amount;
                yield return payment.PaymentDate;
                yield return payment.EarningType;
            }
        }

        private static Incentive Create(
            Phase phase,
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate)
        {
            var paymentProfiles = GetPaymentProfiles(phase, dateOfBirth, startDate, incentivePaymentProfiles);

            if (phase == Phase.Phase1)
            {
                return new Phase1Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning, submissionDate);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning, submissionDate);
            }

            return null; // wouldn't get here
        }
    }
}