using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public abstract class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        protected readonly DateTime StartDate;
        private readonly IncentiveType _incentiveType;
        private readonly IReadOnlyCollection<BreakInLearning> _breaksInLearning;
        private readonly DateTime _submissionDate;
        public static readonly List<EarningType> EarningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };
        public IReadOnlyCollection<Payment> Payments => Generate(_incentiveType, _breaksInLearning, _submissionDate).AsReadOnly();
        public IncentiveType IncentiveType => _incentiveType;
        public abstract bool IsEligible { get; }
        public abstract List<PaymentProfile> PaymentProfiles { get; }
        public abstract List<EligibilityPeriod> EligibilityPeriods { get; }

        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 04, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2021, 09, 30);

        protected abstract int? DelayPeriod { get; }

        protected abstract DateTime CalculateMinimumDueDate(PaymentProfile paymentProfile, DateTime submissionDate);

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _incentiveType = incentiveType;
            _breaksInLearning = breaksInLearning;
            _submissionDate = submissionDate;
        }

        public static Incentive Create(
            ApprenticeshipIncentive incentive)
        {
            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, incentive.BreakInLearnings, incentive.SubmissionDate);
        }

        public static Incentive Create(IncentiveApplicationApprenticeship application)
        {
            return Create(application.Phase, application.DateOfBirth, application.PlannedStartDate, new Collection<BreakInLearning>(), DateTime.UtcNow);
        }

        public bool IsNewAgreementRequired(int signedAgreementVersion)
        {
            if (!IsEligible)
            {
                return false;
            }
            var applicablePeriod = EligibilityPeriods.Single(x => x.StartDate <= StartDate && x.EndDate >= StartDate);
            return signedAgreementVersion < applicablePeriod.MinimumAgreementVersion;
        }

        public static bool IsNewAgreementRequired(IncentiveApplication application)
        {
            foreach (var apprenticeship in application.Apprenticeships)
            {
                var incentive = Create(apprenticeship);

                if (incentive.IsNewAgreementRequired(application.LegalEntity.SignedAgreementVersion ?? 0))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool StartDatesAreEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase == Phase.NotSet)
            {
                return false;
            }

            if (apprenticeship.Phase == Phase.Phase1)
            {
                return Phase1Incentive.StartDatesAreEligible(apprenticeship);
            }
            else if(apprenticeship.Phase == Phase.Phase2)
            {
                return Phase2Incentive.StartDatesAreEligible(apprenticeship);
            }

            return Phase3Incentive.StartDatesAreEligible(apprenticeship);
		}

        public int MinimumDaysInLearning(EarningType earningType)
        {
            var paymentProfile = PaymentProfiles.ElementAt(EarningTypes.IndexOf(earningType));
            return paymentProfile.DaysAfterApprenticeshipStart;
        }
        
        public decimal GetTotalIncentiveAmount()
        {
            return PaymentProfiles.Where(x => x.IncentiveType == _incentiveType).Sum(profile => profile.AmountPayable);
        }

        private static int AgeAtStartOfCourse(DateTime dateOfBirth, DateTime startDate)
        {
            return dateOfBirth.AgeOnThisDay(startDate);
        }

        protected List<Payment> Generate(IncentiveType incentiveType, IReadOnlyCollection<BreakInLearning> breaksInLearning, DateTime submissionDate)
        {
            var paymentProfiles = PaymentProfiles.Where(x => x.IncentiveType == incentiveType).ToList();
            if (!paymentProfiles.Any())
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {incentiveType}");
            }

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
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate)
        {

            var incentiveType = AgeAtStartOfCourse(dateOfBirth, startDate) >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

            if (phase == Phase.Phase1)
            {
                return new Phase1Incentive(dateOfBirth, startDate, incentiveType, breaksInLearning, submissionDate);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, incentiveType, breaksInLearning, submissionDate);
            }
            else if (phase == Phase.Phase3)
            {
                return new Phase3Incentive(dateOfBirth, startDate, incentiveType, breaksInLearning, submissionDate);
            }

            return new PhaseNotSetIncentive(dateOfBirth, startDate, incentiveType, breaksInLearning, submissionDate);
        }
    }
    
}