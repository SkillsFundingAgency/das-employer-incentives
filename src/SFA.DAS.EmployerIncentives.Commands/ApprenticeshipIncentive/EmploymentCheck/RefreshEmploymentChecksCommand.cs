using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck
{
    public class RefreshEmploymentChecksCommand : DomainCommand, ILogWriter
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
    }
}
