using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivePaymentProfile : ValueObject
    {
        public IncentivePaymentProfile(
            IncentivePhase phase,
            byte minAgreementVersion,
            DateTime applicationStartDate,
            DateTime applicationEndDate,
            DateTime trainingStartDate,
            DateTime trainingEndDate,
            IList<PaymentProfile> paymentProfiles
            )
        {
            IncentivePhase = phase;
            MinRequiredAgreementVersion = minAgreementVersion;
            PaymentProfiles = paymentProfiles;
            EligibleApplicationDates = (applicationStartDate, applicationEndDate);
            EligibleTrainingDates = (trainingStartDate, trainingEndDate);
        }

        public IncentivePhase IncentivePhase { get; }
        public IList<PaymentProfile> PaymentProfiles { get; }
        public byte MinRequiredAgreementVersion { get; }
        public (DateTime Start, DateTime End) EligibleApplicationDates { get;  }
        public (DateTime Start, DateTime End) EligibleTrainingDates { get;}

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return IncentivePhase;
            yield return MinRequiredAgreementVersion;
            yield return EligibleApplicationDates;
            yield return EligibleTrainingDates;
            foreach (var paymentProfile in PaymentProfiles)
            {
                yield return paymentProfile;
            }
        }
    }
}