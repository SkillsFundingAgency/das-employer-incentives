using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.PaymentProcess
{
    public class SendMetricsReportEmailCommand : DomainCommand, ILogWriterWithArgs
    {
        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; private set; }

        public string EmailAddress { get; private set; }

        public string ApprovalLink { get; private set; }

        public SendMetricsReportEmailCommand(
            Domain.ValueObjects.CollectionPeriod collectionPeriod,
            string emailAddress,
            string approvalLink)
        {
            CollectionPeriod = collectionPeriod;
            EmailAddress = emailAddress;
            ApprovalLink = approvalLink;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>("Sending metrics report email", new object[] { }),
                    OnProcessed = () => new Tuple<string, object[]>("Metrics report sent to {emailAddress} {collectionPeriod}", new object[] { EmailAddress, CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>("Metrics report email sending failed", new object[] { })
                };
            }
        }
    }
}