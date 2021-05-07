﻿using SFA.DAS.EmployerIncentives.Abstractions.Domain;
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

        public static IncentivePhase Create(DateTime applicationSubmissionDate)
        {
            if (applicationSubmissionDate < new DateTime(2021, 6, 1))
            {
                return new IncentivePhase(Phase.Phase1);
            }
            else if(applicationSubmissionDate < new DateTime(2021, 12, 1))
            {
                return new IncentivePhase(Phase.Phase2);
            }

            return new IncentivePhase(Phase.NotSet);
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Identifier;
        }
    }
}
