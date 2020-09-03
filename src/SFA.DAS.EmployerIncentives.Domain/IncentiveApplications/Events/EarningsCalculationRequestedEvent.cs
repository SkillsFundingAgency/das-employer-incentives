using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerIncentives.Abstractions.Logging;
using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EarningsCalculationRequestedEvent : IDomainEvent, ILogWriter<EarningsCalculationRequestedEvent>
    {
        public long AccountId { get; set; }
        public Guid IncentiveClaimApplicationId { get; set; }
        public Guid ApprenticeshipId { get; set; }
        public IncentiveType IncentiveType { get; set; }
        public DateTime ApprenticeshipStartDate { get; set; }

        public void Write(ILogger<EarningsCalculationRequestedEvent> logger)
        {
            logger.LogInformation($"EarningsCalculationRequestedEvent with AccountId {AccountId}, IncentiveClaimApplicationId {IncentiveClaimApplicationId} and ApprenticeshipId {ApprenticeshipId}");
        }
    }
}
