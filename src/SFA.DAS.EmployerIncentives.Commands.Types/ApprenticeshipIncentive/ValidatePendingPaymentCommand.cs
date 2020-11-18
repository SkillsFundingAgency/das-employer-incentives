using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class ValidatePendingPaymentCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PendingPaymentId { get; }
        public short CollectionYear { get; set; }
        public byte CollectionMonth { get; set; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public ValidatePendingPaymentCommand(
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            short collectionYear,
            byte collectionMonth)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            CollectionYear = collectionYear;
            CollectionMonth = collectionMonth;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive ValidatePendingPaymentCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId} and PendingPaymentId {PendingPaymentId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
