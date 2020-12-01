using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment
{
    public class CreatePaymentCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PendingPaymentId { get; }
        public short CollectionYear { get; }
        public Byte CollectionPeriod { get; }
        public string LockId { get => $"{nameof(ApprenticeshipIncentiveId)}_{ApprenticeshipIncentiveId}"; }

        public CreatePaymentCommand(Guid apprenticeshipIncentiveId, Guid pendingPaymentId, short collectionYear, byte collectionPeriod)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            CollectionYear = collectionYear;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"IncentiveApplications CreatePaymentCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, PendingPaymentId {PendingPaymentId}, CollectionYear {CollectionYear} and CollectionMonth {CollectionPeriod}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
