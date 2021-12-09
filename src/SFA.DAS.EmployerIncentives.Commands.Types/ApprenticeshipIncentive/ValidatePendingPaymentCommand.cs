using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class ValidatePendingPaymentCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PendingPaymentId { get; }
        public short CollectionYear { get; set; }
        public byte CollectionPeriod { get; set; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public ValidatePendingPaymentCommand(
            Guid apprenticeshipIncentiveId,
            Guid pendingPaymentId,
            short collectionYear,
            byte collectionPeriod)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            CollectionYear = collectionYear;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "Pending Payment [PendingPaymentId={pendingPaymentId}], [collection period={year}/{period}], [ApprenticeshipIncentiveId={apprenticeshipIncentiveId}]";

                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>("Validating " + message, new object[] { PendingPaymentId, CollectionYear, CollectionPeriod, ApprenticeshipIncentiveId }),
                    OnProcessed = () => new Tuple<string, object[]>("Validated " + message, new object[] { PendingPaymentId, CollectionYear, CollectionPeriod, ApprenticeshipIncentiveId }),
                    OnError = () => new Tuple<string, object[]>("Error validating " + message, new object[] { PendingPaymentId, CollectionYear, CollectionPeriod, ApprenticeshipIncentiveId })
                };
            }
        }
    }
}
