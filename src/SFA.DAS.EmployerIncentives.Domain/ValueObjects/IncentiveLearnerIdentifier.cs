using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentiveLearnerIdentifier : ValueObject
    {
        public long AccountLegalEntityId { get; }
        public long ULN { get; }

        public IncentiveLearnerIdentifier(long accountLegalEntityId, long uln)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ULN = uln;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return AccountLegalEntityId;
            yield return ULN;
        }
    }
}
