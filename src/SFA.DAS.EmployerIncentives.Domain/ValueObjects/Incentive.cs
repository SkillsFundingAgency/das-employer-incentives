using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
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

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _payments = Generate(paymentProfiles, breaksInLearning);
        }
        
        public static async Task<Incentive> Create(
            ApprenticeshipIncentive incentive,            
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            var paymentProfiles = incentivePaymentProfilesService.Get();

            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, paymentProfiles, incentive.BreakInLearnings);            
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

        protected List<Payment> Generate(IEnumerable<PaymentProfile> paymentProfiles, IReadOnlyCollection<BreakInLearning> breaksInLearning)
        {
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
            IEnumerable<IncentivePaymentProfile> incentivePaymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning)
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

            if (phase == Phase.Phase1)
            {
                return new Phase1Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning);
            }

            return null; // wouldn't get here
        }
    }

    public class Phase1Incentive : Incentive
    {
        public Phase1Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning) : base(dateOfBirth, startDate, paymentProfiles, breaksInLearning)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

        private static List<EligibilityPeriod> EligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibilityPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public static int MinimumAgreementVersion(DateTime startDate)
        {
            var applicablePeriod = EligibilityPeriods.SingleOrDefault(x => x.StartDate <= startDate && x.EndDate >= startDate);
            return applicablePeriod?.MinimumAgreementVersion ?? EligibilityPeriods.First().MinimumAgreementVersion;
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
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breakInLearningDayCount) : base(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount)
        {
        }

        public static DateTime EligibilityStartDate = new DateTime(2021, 4, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 11, 30);

        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 04, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2021, 11, 30);

        public override bool IsEligible => StartDate >= EligibilityStartDate && StartDate <= EligibilityEndDate;

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