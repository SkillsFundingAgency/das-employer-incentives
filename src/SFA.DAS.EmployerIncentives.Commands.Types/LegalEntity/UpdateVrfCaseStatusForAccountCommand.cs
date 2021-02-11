using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity
{
    public class UpdateVrfCaseStatusForAccountCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public long AccountId { get; }
        public string CaseId { get; }
        public string Status { get; }
        public DateTime LastUpdatedDate { get; }
        public string HashedLegalEntityId { get; }
        public string LockId => $"{nameof(Domain.Accounts.Account)}_{AccountId}";

        public UpdateVrfCaseStatusForAccountCommand(long accountId, string hashedLegalEntityId, string caseId, string status, DateTime lastUpdatedDate)
        {
            AccountId = accountId;
            HashedLegalEntityId = hashedLegalEntityId;
            CaseId = caseId;
            Status = status;
            LastUpdatedDate = lastUpdatedDate;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"Account {nameof(UpdateVrfCaseStatusForAccountCommand)} event with AccountId {AccountId}, " +
                              $"CaseId {CaseId}, Status {Status}, LastUpdatedDate {LastUpdatedDate}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
