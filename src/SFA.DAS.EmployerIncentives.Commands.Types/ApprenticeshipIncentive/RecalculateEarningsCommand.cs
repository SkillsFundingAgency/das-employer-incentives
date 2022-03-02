using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class RecalculateEarningsCommand : DomainCommand, IPeriodEndIncompatible
    {
        public IEnumerable<IncentiveLearnerIdentifierDto> IncentiveLearnerIdentifiers { get; }

        public RecalculateEarningsCommand(IEnumerable<IncentiveLearnerIdentifierDto> incentiveLearnerIdentifiers)
        {
            IncentiveLearnerIdentifiers = incentiveLearnerIdentifiers;
        }

        public TimeSpan CommandDelay => TimeSpan.FromHours(1);
        public bool CancelCommand => false;
    }
}
