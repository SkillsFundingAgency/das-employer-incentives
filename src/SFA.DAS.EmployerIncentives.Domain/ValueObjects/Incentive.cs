using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        protected abstract bool IsNewAgreementRequired(int signedAgreementVersion, DateTime employmentStartDate);
        protected abstract int? DelayPeriod { get; }

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IncentiveType incentiveType,
            IReadOnlyCollection<BreakInLearning> breaksInLearning,
            DateTime submissionDate)
        {
            _dateOfBirth = dateOfBirth;
            StartDate = startDate;
            _payments = Generate(incentiveType, breaksInLearning, submissionDate);
        }

        public static Incentive Create(
            ApprenticeshipIncentive incentive)
        {
            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, incentive.BreakInLearnings, incentive.SubmissionDate);
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

                if (apprenticeship.EmploymentStartDate.HasValue &&
                    incentive.IsNewAgreementRequired(application.LegalEntity.SignedAgreementVersion ?? 0, apprenticeship.EmploymentStartDate.Value))
                {
                    return true;
                }
            }

            return false;
        }

        public static Incentive Create(IncentiveApplicationApprenticeshipDto application)
        {
            return Create(application.Phase, application.DateOfBirth, application.PlannedStartDate, new Collection<BreakInLearning>(), DateTime.UtcNow);
        }

        public static bool EmployerStartDateIsEligible(Apprenticeship apprenticeship)
        {
            if (apprenticeship.Phase == Phase.Phase1)
            {
                return Phase1Incentive.EmployerStartDateIsEligible(apprenticeship);
            }
            else if(apprenticeship.Phase == Phase.Phase2)
            {
                return Phase2Incentive.EmployerStartDateIsEligible(apprenticeship);
            }
            return Phase3Incentive.EmployerStartDateIsEligible(apprenticeship);
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
                var paymentDueDate = CalculateDueDate(paymentProfile, submissionDate);
                payments.Add(new Payment(paymentProfile.AmountPayable, paymentDueDate, _earningTypes[paymentIndex], breaksInLearning));
                paymentIndex++;
            }

            return payments;
        }

        private DateTime CalculateDueDate(PaymentProfile paymentProfile, DateTime submissionDate)
        {
            if (DelayPeriod == null)
            {
                return StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
            }

            var minimumDueDate = submissionDate.Date.AddDays(DelayPeriod.Value);

            var paymentDueDate = StartDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart);
            if (paymentDueDate < minimumDueDate)
            {
                paymentDueDate = minimumDueDate;
            }

            return paymentDueDate;
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

            return null; // wouldn't get here
        }
    }
    
}