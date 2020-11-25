using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.SendPaymentRequests
{
    public class SendPaymentRequestsCommand : DomainCommand, ILogWriter
    {
        public long AccountLegalEntityId { get; }
        public DateTime PaidDate { get; }

        public SendPaymentRequestsCommand(long accountLegalEntityId, DateTime paidDate)
        {
            AccountLegalEntityId = accountLegalEntityId;
            PaidDate = paidDate;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"IncentiveApplications SendPaymentRequestsCommand for AccountLegalEntity {AccountLegalEntityId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
