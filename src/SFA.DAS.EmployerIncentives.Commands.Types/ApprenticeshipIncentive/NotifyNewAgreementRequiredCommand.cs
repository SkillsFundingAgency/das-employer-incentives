using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class NotifyNewAgreementRequiredCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public const string TemplateId = "NewAgreementVersionNeedsAccepting";
        public string HashedAccountId { get; }
        public string LegalEntityName { get; }
        public string LockId => $"{nameof(Account)}_{HashedAccountId}";

        public NotifyNewAgreementRequiredCommand(string hashedAccountId, string legalEntityName)
        {
            HashedAccountId = hashedAccountId;
            LegalEntityName = legalEntityName;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message =
                    $"ApprenticeshipIncentive NotifyNewAgreementRequiredCommand for AccountId {HashedAccountId}, " +
                    $"LegalEntityName {LegalEntityName} ";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
