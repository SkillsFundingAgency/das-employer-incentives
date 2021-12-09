using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment
{
    public class CreatePaymentCommand : DomainCommand, ILockIdentifier, ILogWriterWithArgs
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public Guid PendingPaymentId { get; }
        public Domain.ValueObjects.CollectionPeriod CollectionPeriod { get; }
        public string LockId { get => $"{nameof(ApprenticeshipIncentiveId)}_{ApprenticeshipIncentiveId}"; }

        public CreatePaymentCommand(Guid apprenticeshipIncentiveId, Guid pendingPaymentId, Domain.ValueObjects.CollectionPeriod collectionPeriod)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            PendingPaymentId = pendingPaymentId;
            CollectionPeriod = collectionPeriod;
        }

        [Newtonsoft.Json.JsonIgnore]
        public LogWithArgs Log
        {
            get
            {
                var message = "IncentiveApplications CreatePaymentCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, PendingPaymentId {PendingPaymentId}, CollectionPeriod {CollectionPeriod}";
                return new LogWithArgs
                {
                    OnProcessing = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId, PendingPaymentId, CollectionPeriod }),
                    OnProcessed = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId, PendingPaymentId, CollectionPeriod }),
                    OnError = () => new Tuple<string, object[]>(message, new object[] { ApprenticeshipIncentiveId, PendingPaymentId, CollectionPeriod })
                };
            }
        }
    }
}
