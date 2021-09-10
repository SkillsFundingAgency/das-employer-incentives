using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class AgreementVersion : ValueObject
    {
        public int? MinimumRequiredVersion { get; }

        public AgreementVersion(int? minimumRequiredVersion)
        {
            MinimumRequiredVersion = minimumRequiredVersion;
        }
        
        public static AgreementVersion Create(Phase phase, DateTime startDate)
        {
            int minimumAgreementVersion;

            if (phase == Phase.Phase1)
            {
                minimumAgreementVersion = Phase1Incentive.MinimumAgreementVersion(startDate);
            }
            else if (phase == Phase.Phase2)
            {
                minimumAgreementVersion = Phase2Incentive.MinimumAgreementVersion(startDate);
            }
            else
            {
                minimumAgreementVersion = Phase3Incentive.MinimumAgreementVersion(startDate);
            }

            return new AgreementVersion(minimumAgreementVersion);
        }

        public AgreementVersion ChangedStartDate(Phase phase, DateTime startDate)
        {
            var newVersion = Create(phase, startDate);

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
