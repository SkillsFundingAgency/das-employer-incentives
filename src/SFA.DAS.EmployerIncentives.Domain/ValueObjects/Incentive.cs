using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
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

        private static readonly DateTime EmployerEligibilityStartDate = new DateTime(2021, 04, 01);
        private static readonly DateTime EmployerEligibilityEndDate = new DateTime(2021, 09, 30);

        private const int DelayPeriod = 21;

        protected Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            IDateTimeService dateTimeService)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _payments = Generate(paymentProfiles, breaksInLearning, dateTimeService);
        }
        
        public static async Task<Incentive> Create(
            ApprenticeshipIncentive incentive,            
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            IDateTimeService dateTimeService)
        {
            var paymentProfiles = await incentivePaymentProfilesService.Get();

            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, paymentProfiles, incentive.BreakInLearnings, dateTimeService);            
        }        

        public static async Task<Incentive> Create(
            IncentiveApplicationApprenticeshipDto incentiveApplication,
            IIncentivePaymentProfilesService incentivePaymentProfilesService,
            IDateTimeService dateTimeService)
        {
            var paymentProfiles = await incentivePaymentProfilesService.Get();
            return Create(incentiveApplication.Phase, incentiveApplication.DateOfBirth, incentiveApplication.PlannedStartDate, paymentProfiles, new List<BreakInLearning>(), dateTimeService);
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

        private static int AgeAtStartOfCourse(DateTime dateOfBirth, DateTime startDate)
        {
            return dateOfBirth.AgeOnThisDay(startDate);
        }

        protected List<Payment> Generate(IEnumerable<PaymentProfile> paymentProfiles, IReadOnlyCollection<BreakInLearning> breaksInLearning, IDateTimeService dateTimeService)
        {
            var payments = new List<Payment>();
            if (!IsEligible) return payments;

            var minimumDueDate = dateTimeService.Now().AddDays(DelayPeriod);

            var paymentIndex = 0;
            foreach (var paymentProfile in paymentProfiles)
            {
                var paymentDueDate = StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
                if (paymentDueDate < minimumDueDate)
                {
                    paymentDueDate = minimumDueDate;
                }
                payments.Add(new Payment(paymentProfile.AmountPayable, paymentDueDate, _earningTypes[paymentIndex], breaksInLearning));
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
            IDateTimeService dateTimeService)
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
                return new Phase1Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning, dateTimeService);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, paymentProfiles, breaksInLearning, dateTimeService);
            }

            return null; // wouldn't get here
        }
    }
}