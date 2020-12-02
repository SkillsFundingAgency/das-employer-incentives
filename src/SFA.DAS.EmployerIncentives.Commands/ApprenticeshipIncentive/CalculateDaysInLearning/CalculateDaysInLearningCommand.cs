using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CalculateDaysInLearning
{
    public class CalculateDaysInLearningCommand : DomainCommand, ILockIdentifier, ILogWriter
    {
        public Guid ApprenticeshipIncentiveId { get; }
        public byte CollectionPeriodNumber { get; }
        public short CollectionYear { get; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public CalculateDaysInLearningCommand(Guid apprenticeshipIncentiveId, byte collectionPeriodNumber, short collectionYear)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
            CollectionPeriodNumber = collectionPeriodNumber;
            CollectionYear = collectionYear;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive CalculateDaysInLearningCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}, CollectionPeriodNumber {CollectionPeriodNumber}, CollectionYear {CollectionYear} ";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }
    }
}
