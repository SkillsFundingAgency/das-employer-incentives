using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Events
{
    public class AccountLegalEntityRemoved : IDomainEvent, ILogWriter
    {
        public long AccountLegalEntityId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"AccountLegalEntityRemoved event with AccountLegalEntityId {AccountLegalEntityId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
