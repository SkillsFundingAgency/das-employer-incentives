using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommand : DomainCommand, ILogWriter
    {
        public long AccountLegalEntity { get; }

        public SendPaymentRequestsCommand(long accountLegalEntity)
        {
            AccountLegalEntity = accountLegalEntity;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"IncentiveApplications SendPaymentRequestsCommand for AccountLegalEntity {AccountLegalEntity}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
