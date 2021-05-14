﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public abstract class Incentive : ValueObject
    {
        private readonly DateTime _dateOfBirth;
        private readonly DateTime _startDate;
        private readonly List<Payment> _payments;
        private readonly int _breakInLearningDayCount;
        private readonly List<EarningType> _earningTypes = new List<EarningType> { EarningType.FirstPayment, EarningType.SecondPayment };

        private static List<EligibilityPeriod> EligibilityPeriods = new List<EligibilityPeriod>
        {
            new EligibilityPeriod(new DateTime(2020, 8, 1), new DateTime(2021, 1, 31), 4),
            new EligibilityPeriod(new DateTime(2021, 2, 1), new DateTime(2021, 5, 31), 5)
        };

        public static DateTime EligibilityStartDate = new DateTime(2020, 8, 1);
        public static DateTime EligibilityEndDate = new DateTime(2021, 5, 31);
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        public bool IsEligible => _startDate >= EligibilityStartDate && _startDate <= EligibilityEndDate;

        public static DateTime MinimumCommitmentStartDate = new DateTime(2021, 4, 1);
        public static DateTime MaximumCommitmentStartDate = new DateTime(2021, 11, 30);

        protected Incentive(
            DateTime dateOfBirth, 
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            int breakInLearningDayCount)
        {
            _dateOfBirth = dateOfBirth;
            _startDate = startDate;
            _breakInLearningDayCount = breakInLearningDayCount;
            _payments = Generate(paymentProfiles, _breakInLearningDayCount);
        }
        
        public static async Task<Incentive> Create(
            ApprenticeshipIncentive incentive,            
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            var paymentProfiles = await incentivePaymentProfilesService.Get();

            return Create(incentive.Phase.Identifier, incentive.Apprenticeship.DateOfBirth, incentive.StartDate, paymentProfiles, incentive.BreakInLearningDayCount);            
        }        

        public static async Task<Incentive> Create(
            IncentiveApplicationApprenticeshipDto incentiveApplication,
            IIncentivePaymentProfilesService incentivePaymentProfilesService)
        {
            var paymentProfiles = await incentivePaymentProfilesService.Get();
            return Create(incentiveApplication.Phase, incentiveApplication.DateOfBirth, incentiveApplication.PlannedStartDate, paymentProfiles, 0);
        }

        public static bool IsNewAgreementRequired(IncentiveApplicationDto application, LegalEntityDto legalEntityDto)
        {
            if(application.AccountLegalEntityId != legalEntityDto.AccountLegalEntityId)
            {
                throw new ArgumentException($"Legal entity {legalEntityDto.AccountLegalEntityId} is not related to the application {application.AccountLegalEntityId} when checking IsNewAgreementRequired");
            }

            foreach (var apprenticeship in application.Apprenticeships)
            {
                if (IsNewAgreementRequired(apprenticeship, legalEntityDto.SignedAgreementVersion ?? 0))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNewAgreementRequired(IncentiveApplicationApprenticeshipDto application, int signedAgreementVersion)
        {
            var isEligible = application.PlannedStartDate >= EligibilityStartDate && application.PlannedStartDate <= EligibilityEndDate;
            if (!isEligible)
            {
                return true;
            }
            var applicablePeriod = EligibilityPeriods.Single(x => x.StartDate <= application.PlannedStartDate && x.EndDate >= application.PlannedStartDate);
            return signedAgreementVersion < applicablePeriod.MinimumAgreementVersion;
        }

        private static int AgeAtStartOfCourse(DateTime dateOfBirth, DateTime startDate)
        {
            return dateOfBirth.AgeOnThisDay(startDate);
        }

        protected List<Payment> Generate(IEnumerable<PaymentProfile> paymentProfiles, int breakInLearningDayCount)
        {
            var payments = new List<Payment>();

            if (!IsEligible)
            {
                return payments;
            }          

            var paymentIndex = 0;
            foreach (var paymentProfile in paymentProfiles)
            {
                payments.Add(new Payment(paymentProfile.AmountPayable, _startDate.AddDays(paymentProfile.DaysAfterApprenticeshipStart).AddDays(breakInLearningDayCount), _earningTypes[paymentIndex]));
                paymentIndex++;
            }

            return payments;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return _dateOfBirth;
            yield return _startDate;
            yield return _breakInLearningDayCount;

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
            int breakInLearningDayCount)
        {
            var incentivePaymentProfile = incentivePaymentProfiles.FirstOrDefault(x => x.IncentivePhase.Identifier == phase);

            if (incentivePaymentProfile?.PaymentProfiles == null)
            {
                throw new MissingPaymentProfileException($"Incentive Payment profile not found for IncentivePhase {phase}");
            }

            var incentiveType = AgeAtStartOfCourse(dateOfBirth, startDate) >= 25 ? IncentiveType.TwentyFiveOrOverIncentive : IncentiveType.UnderTwentyFiveIncentive;

            var paymentProfiles = incentivePaymentProfile.PaymentProfiles.Where(x => x.IncentiveType == incentiveType);

            if (!paymentProfiles.Any())
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {incentiveType}");
            }

            if (phase == Phase.Phase1)
            {
                return new Phase1Incentive(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount);
            }
            else if (phase == Phase.Phase2)
            {
                return new Phase2Incentive(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount);
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
            int breakInLearningDayCount) : base(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount)
        {
        }

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
    }

    public class Phase2Incentive : Incentive
    {
        public Phase2Incentive(
            DateTime dateOfBirth,
            DateTime startDate,
            IEnumerable<PaymentProfile> paymentProfiles,
            int breakInLearningDayCount) : base(dateOfBirth, startDate, paymentProfiles, breakInLearningDayCount)
        {
        }

        public static int MinimumAgreementVersion(DateTime startDate) => 6;
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