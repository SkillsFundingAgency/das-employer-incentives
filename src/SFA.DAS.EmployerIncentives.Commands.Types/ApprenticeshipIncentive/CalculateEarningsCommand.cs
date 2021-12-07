using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System;

namespace SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive
{
    public class CalculateEarningsCommand : DomainCommand, ILockIdentifier, ILogWriter, IPeriodEndIncompatible
    {
        public Guid ApprenticeshipIncentiveId { get; private set; }

        public string LockId { get => $"{nameof(Domain.ApprenticeshipIncentives.ApprenticeshipIncentive)}_{ApprenticeshipIncentiveId}"; }

        public CalculateEarningsCommand(
            Guid apprenticeshipIncentiveId)
        {
            ApprenticeshipIncentiveId = apprenticeshipIncentiveId;
        }

        [Newtonsoft.Json.JsonIgnore]
        public Log Log
        {
            get
            {
                var message = $"ApprenticeshipIncentive CalculateEarningsCommand for ApprenticeshipIncentiveId {ApprenticeshipIncentiveId}";
                return new Log
                {
                    OnProcessing = () => message,
                    OnError = () => message
                };
            }
        }

        public TimeSpan CommandDelay => TimeSpan.FromHours(1);
        public bool CancelCommand => false;
    }
}
