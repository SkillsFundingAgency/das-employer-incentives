using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Events
{
    public class BankDetailsApprovedForLegalEntity : IDomainEvent, ILogWriter
    {
        public string HashedLegalEntityId { get; set; }

        public Log Log
        {
            get
            {
                var message = $"Account BankDetailsApprovedForLegalEntity event with LegalEntityId {HashedLegalEntityId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
