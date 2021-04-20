using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivePaymentProfile : ValueObject
    {
        public IncentivePaymentProfile(
            IncentiveType type,
            IncentivePhase phase,
            byte minAgreementVersion,
            DateTime applicationStartDate,
            DateTime applicationEndDate,
            DateTime employmentStartDate,
            DateTime employmentEndDate,
            DateTime trainingStartDate,
            DateTime trainingEndDate,
            List<PaymentProfile> paymentProfiles
            )
        {
            IncentiveType = type;
            IncentivePhase = phase;
            MinRequiredAgreementVersion = minAgreementVersion;
            PaymentProfiles = paymentProfiles;
            EligibleApplicationDates = (applicationStartDate, applicationEndDate);
            EligibleEmploymentDates = (employmentStartDate, employmentEndDate);
            EligibleTrainingDates = (trainingStartDate, trainingEndDate);
        }

        public IncentiveType IncentiveType { get; }
        public IncentivePhase IncentivePhase { get; }
        public List<PaymentProfile> PaymentProfiles { get; }
        public byte MinRequiredAgreementVersion { get; }
        public (DateTime Start, DateTime End) EligibleApplicationDates { get;  }
        public (DateTime Start, DateTime End) EligibleEmploymentDates { get; }
        public (DateTime Start, DateTime End) EligibleTrainingDates { get;}

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return IncentiveType;
            yield return IncentivePhase;
            yield return MinRequiredAgreementVersion;
            yield return EligibleApplicationDates;
            yield return EligibleEmploymentDates;
            yield return EligibleTrainingDates;
            foreach (var paymentProfile in PaymentProfiles)
            {
                yield return paymentProfile;
            }
        }

    }
}