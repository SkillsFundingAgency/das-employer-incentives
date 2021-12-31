using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Commands.Types;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommand : DomainCommand, ILogWriter, IPeriodEndIncompatible
    {
        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"RefreshEmploymentChecksCommand";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }

        public TimeSpan CommandDelay { get; }
        public bool CancelCommand => true;
    }
}
