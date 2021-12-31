using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types
{
    public interface IPeriodEndIncompatible
    {
        TimeSpan CommandDelay { get; }
        bool CancelCommand { get; }
    }
}
