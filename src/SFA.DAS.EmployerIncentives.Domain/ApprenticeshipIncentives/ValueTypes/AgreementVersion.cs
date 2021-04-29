using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class AgreementVersion : ValueObject
    {
        private const int MinimumEmployerIncentivesAgreementVersion = 4;
        private const int SchemeEligibilityExtensionAgreementVersion = 5;
        public int? MinimumRequiredVersion { get; }

        public AgreementVersion(int? minimumRequiredVersion)
        {
            MinimumRequiredVersion = minimumRequiredVersion;
        }
        
        public static AgreementVersion Create(DateTime startDate)
        {
            var schemeEligibilityExtensionStartDate = new DateTime(2021, 02, 01);
            int minimumAgreementVersion = SchemeEligibilityExtensionAgreementVersion;
            if (startDate < schemeEligibilityExtensionStartDate)
            {
                minimumAgreementVersion = MinimumEmployerIncentivesAgreementVersion;
            }

            return new AgreementVersion(minimumAgreementVersion);
        }

        public AgreementVersion ChangedStartDate(DateTime startDate)
        {
            var schemeEligibilityExtensionEndDate = new DateTime(2021, 05, 31);

            if (startDate > schemeEligibilityExtensionEndDate)
            {
                return this; // no need to change the version outside the eligibility window 
            }

            var newVersion = Create(startDate);

            if(newVersion.MinimumRequiredVersion > MinimumRequiredVersion)
            {
                return newVersion; // only increase the version on start date change
            }

            return this;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return MinimumRequiredVersion;
        }
    }
}
