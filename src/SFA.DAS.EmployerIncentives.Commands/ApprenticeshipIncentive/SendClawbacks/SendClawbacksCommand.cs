using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendClawbacks
{
    public class SendClawbacksCommand : DomainCommand, ILogWriter
    {
        public long AccountLegalEntityId { get; }
        public DateTime ClawbackDate { get; }

        public SendClawbacksCommand(long accountLegalEntityId, DateTime clawbackDate)
        {
            AccountLegalEntityId = accountLegalEntityId;
            ClawbackDate = clawbackDate;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"IncentiveApplications SendClawbacksCommand for AccountLegalEntity {AccountLegalEntityId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
