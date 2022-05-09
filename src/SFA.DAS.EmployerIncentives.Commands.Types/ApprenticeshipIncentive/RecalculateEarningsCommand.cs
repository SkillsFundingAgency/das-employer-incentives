using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class RecalculateEarningsCommand : DomainCommand, IPeriodEndIncompatible
    {
        public IEnumerable<IncentiveLearnerIdentifier> IncentiveLearnerIdentifiers { get; }

        public RecalculateEarningsCommand(IEnumerable<IncentiveLearnerIdentifier> incentiveLearnerIdentifiers)
        {
            IncentiveLearnerIdentifiers = incentiveLearnerIdentifiers;
        }

        public TimeSpan CommandDelay => TimeSpan.FromHours(1);
        public bool CancelCommand => false;
    }
}
