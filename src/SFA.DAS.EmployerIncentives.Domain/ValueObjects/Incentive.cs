using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;
using Apprenticeship = SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Apprenticeship;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public abstract class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        protected readonly DateTime StartDate;
        private readonly List<Payment> _payments;
        private readonly List<EarningType> _earningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        public abstract bool IsEligible { get; }
        public abstract List<PaymentProfile> PaymentProfiles { get; }
        public abstract List<EligibilityPeriod> EligibilityPeriods { get; }

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breaksInLearning)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _payments = Generate(incentiveType, breaksInLearning);
        }
        
        public static Incentive Create(ApprenticeshipIncentive incentive)
        {
            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, incentive.BreakInLearnings);            
        }

        public bool IsNewAgreementRequired(int signedAgreementVersion)
        {
            if (!IsEligible)
            {
                return true;
            }
            var applicablePeriod = EligibilityPeriods.Single(x => x.StartDate <= StartDate && x.EndDate >= StartDate);
            return signedAgreementVersion < applicablePeriod.MinimumAgreementVersion;
        }

        public static bool IsNewAgreementRequired(IncentiveApplicationDto application)
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

        public static Incentive Create(IncentiveApplicationApprenticeshipDto application)
        {
            return Create(application.Phase, application.DateOfBirth, application.PlannedStartDate, new Collection<BreakInLearning>());
        }

        public static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase == Phase.Phase1)
            {
                return Phase1Incentive.EmployerStartDateIsEligible(apprenticeship);
            }

            return Phase2Incentive.EmployerStartDateIsEligible(apprenticeship);
        }

        private static int AgeAtStartOfCourse(DateTime dateOfBirth, DateTime startDate)
        {
            return dateOfBirth.AgeOnThisDay(startDate);
        }

        protected List<Payment> Generate(IncentiveType incentiveType, IReadOnlyCollection<BreakInLearning> breaksInLearning)
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
                payments.Add(new Payment(paymentProfile.AmountPayable, StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart), _earningTypes[paymentIndex], breaksInLearning));
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
            IReadOnlyCollection<BreakInLearning> breaksInLearning)
        {

            var incentiveType = AgeAtStartOfCourse(dateOfBirth, startDate) >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

            if (phase == Phase.Phase1)
            {
                return new Phase1Incentive(dateOfBirth, startDate, incentiveType, breaksInLearning);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, incentiveType, breaksInLearning);
            }

            return null; // wouldn't get here
        }
    }

    public class Phase1Incentive : Incentive
    {
        public Phase1Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breaksInLearning) : base(dateOfBirth, startDate, incentiveType, breaksInLearning)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

        public override List<PaymentProfile> PaymentProfiles => 
            new List<PaymentProfile>
            {
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1000),
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1000),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 750),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 750)
            };

        public override List<EligibilityPeriod> EligibilityPeriods => _eligibilityPeriods;

        private static readonly List<EligibilityPeriod> _eligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibilityPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = _eligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? _eligibilityPeriods.First().MinimumAgreementVersion;
        }

        public new static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase != Phase.Phase1)
            {
                throw new InvalidPhaseException();
            }
            return true;
        }
    }

    public class Phase2Incentive : Incentive
    {

        public Phase2Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount) : base(dateOfBirth, startDate, incentiveType, breakInLearningDayCount)
        {
        }
        
        public static DateTime EligibilityStartDate = new DateTime(2021, 4, 1);
        public static DateTime EligibilityEndDate = new DateTime(2022, 1, 31);

        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 04, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2021, 11, 30);

        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

        public override List<PaymentProfile> PaymentProfiles =>
            new List<PaymentProfile>
            {
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                new PaymentProfile(IncentiveType.UnderTwentyFiveIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 89, amountPayable: 1500),
                new PaymentProfile(IncentiveType.TwentyFiveOrOverIncentive, daysAfterApprenticeshipStart: 364, amountPayable: 1500)
            };

        private static readonly List<EligibilityPeriod> _eligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2021, 4, 1), new DateTime(2021, 9, 30), 6),
            new EligibilityPeriod(new DateTime(2021, 10, 1), new DateTime(2022, 1, 31), 7)
        };
        public override List<EligibilityPeriod> EligibilityPeriods => _eligibilityPeriods;

        public static int MinimumAgreementVersion() => 6;
      
        public new static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase != Phase.Phase2)
            {
                throw new InvalidPhaseException();
            }

            if (apprenticeship.EmploymentStartDate.HasValue &&
                (apprenticeship.EmploymentStartDate.Value.Date >= EmployerEligibilityStartDate.Date) &&
                (apprenticeship.EmploymentStartDate.Value.Date <= EmployerEligibilityEndDate.Date))
            {
                return true;
            }

            return false;
        }

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = _eligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? _eligibilityPeriods.First().MinimumAgreementVersion;
        }
    }

    public class EligibilityPeriod
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int MinimumAgreementVersion { get; }

        public EligibilityPeriod(DateTime startDate, DateTime endDate, int minimumAgreementVersion)
        {
            StartDate = startDate;
            EndDate = endDate;
            MinimumAgreementVersion = minimumAgreementVersion;
        }
    }
}