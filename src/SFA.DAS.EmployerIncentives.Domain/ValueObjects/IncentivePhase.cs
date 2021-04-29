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

        public static IncentivePhase Create(DateTime startDate, DateTime applicationSubmissionDate)
        {
            if (applicationSubmissionDate < new DateTime(2021, 6, 1))
            {
                if (startDate >= new DateTime(2020, 8, 1) && startDate <= new DateTime(2021, 1, 31))
                {
                    return new IncentivePhase(Phase.Phase1_0);
                }

                if (startDate >= new DateTime(2021, 2, 1) && startDate <= new DateTime(2021, 3, 31))
                {
                    return new IncentivePhase(Phase.Phase1_1);
                }
            }

            return new IncentivePhase(Phase.NotSet);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Identifier;
        }
    }
}
