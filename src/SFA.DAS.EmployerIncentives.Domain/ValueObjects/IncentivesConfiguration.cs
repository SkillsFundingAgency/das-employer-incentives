using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivesConfiguration
    {
        private readonly IList<IncentivePaymentProfile> _profiles;

        public IncentivesConfiguration(IList<IncentivePaymentProfile> profiles)
        {
            _profiles = profiles;
        }

        public byte GetMinimumAgreementVersion(DateTime trainingStartDate)
        {
            return GetIncentivePhaseConfiguration(trainingStartDate).MinRequiredAgreementVersion;
        }

        public PaymentProfile[] GetPaymentProfiles(IncentiveType incentiveType, DateTime trainingStartDate)
        {
            var phaseConfig = GetIncentivePhaseConfiguration(trainingStartDate);

            var profiles = phaseConfig.PaymentProfiles.Where(x => x.IncentiveType == incentiveType)
                .OrderBy(x => x.DaysAfterApprenticeshipStart).ToArray();

            if (profiles.Any() == false)
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for IncentiveType {incentiveType}");
            }

            return profiles;
        }

        public bool IsEligible(DateTime trainingStartDate)
        {
            return GetIncentivePhaseConfiguration(trainingStartDate) != null;
        }

        private IncentivePaymentProfile GetIncentivePhaseConfiguration(DateTime trainingStartDate)
        {
            var phaseConfig = _profiles.SingleOrDefault(x =>
                x.EligibleTrainingDates.Start <= trainingStartDate && x.EligibleTrainingDates.End >= trainingStartDate
            );

            if (phaseConfig == null)
            {
                throw new MissingPaymentProfileException($"Payment profiles not found for Training Start Date {trainingStartDate}");
            }

            return phaseConfig;
        }
    }
}