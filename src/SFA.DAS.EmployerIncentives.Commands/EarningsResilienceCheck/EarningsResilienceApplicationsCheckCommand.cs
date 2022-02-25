using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types;

namespace SFA.DAS.EmployerIncentives.Commands.EarningsResilienceCheck
{
    public class EarningsResilienceApplicationsCheckCommand : DomainCommand, IPeriodEndIncompatible
    {
        public TimeSpan CommandDelay { get; }
        public bool CancelCommand => true;
    }
}
