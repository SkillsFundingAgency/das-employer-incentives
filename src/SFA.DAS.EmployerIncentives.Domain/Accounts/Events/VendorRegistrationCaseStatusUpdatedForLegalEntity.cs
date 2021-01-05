using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Accounts.Events
{
    public class VendorRegistrationCaseStatusUpdatedForLegalEntity : IDomainEvent, ILogWriter
    {
        public string HashedLegalEntityId { get; }
        public string CaseId { get; }
        public string Status { get; }
        public DateTime LastUpdatedDate { get; }

        public VendorRegistrationCaseStatusUpdatedForLegalEntity(string hashedLegalEntityId,
            string caseId, string status, in DateTime lastUpdatedDate)
        {
            HashedLegalEntityId = hashedLegalEntityId;
            CaseId = caseId;
            Status = status;
            LastUpdatedDate = lastUpdatedDate;
        }

        public Log Log
        {
            get
            {
                var message = $"Account {nameof(VendorRegistrationCaseStatusUpdatedForLegalEntity)} event with LegalEntityId {HashedLegalEntityId}, " +
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