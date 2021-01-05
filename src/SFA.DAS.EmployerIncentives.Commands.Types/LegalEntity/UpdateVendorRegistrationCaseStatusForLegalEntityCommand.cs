using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.LegalEntity
{
    public class UpdateVendorRegistrationCaseStatusForLegalEntityCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public string HashedLegalEntityId { get; }
        public string CaseId { get; }
        public string Status { get; }
        public DateTime LastUpdatedDate { get; }

        public UpdateVendorRegistrationCaseStatusForLegalEntityCommand(string hashedLegalEntityId, string caseId, string status, in DateTime lastUpdatedDate)
        {
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
                var message = $"Account {nameof(UpdateVendorRegistrationCaseStatusForLegalEntityCommand)} event with LegalEntityId {HashedLegalEntityId}, " +
                              $"CaseId {CaseId}, Status {Status}, LastUpdatedDate {LastUpdatedDate}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
        public string LockId => $"{nameof(Domain.Accounts.LegalEntity)}_{HashedLegalEntityId}";
    }
}
