using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class IncentivePhase : ValueObject
    {
        public Phase Identifier { get; }

        public IncentivePhase(Phase identifier)
        {
            Identifier = identifier;
        }

        public static IncentivePhase Create()
        {
            return new IncentivePhase(Phase.Phase2);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Identifier;
        }
    }
}
